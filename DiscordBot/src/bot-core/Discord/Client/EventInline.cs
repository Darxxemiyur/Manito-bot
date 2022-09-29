using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Common.Utilities;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Manito.Discord.Client
{
	/// <summary>
	/// Event inliner.
	/// Intercepts events that are being listened for from existing "sessions"
	/// Pushes non-interesting events further to the EventFilter's buffer
	/// </summary>
	public class EventInline
	{
		public PerEventInline<MessageCreateEventArgs> MessageBuffer {
			get;
		}
		public PerEventInline<InteractionCreateEventArgs> InteractionBuffer {
			get;
		}
		public PerEventInline<ComponentInteractionCreateEventArgs> CompInteractBuffer {
			get;
		}
		public PerEventInline<MessageReactionAddEventArgs> ReactAddBuffer {
			get;
		}
		public PerEventInline<ContextMenuInteractionCreateEventArgs> ContInteractBuffer {
			get;
		}
		private EventBuffer _sourceEventBuffer;
		public EventInline(EventBuffer sourceEventBuffer)
		{
			_sourceEventBuffer = sourceEventBuffer;
			MessageBuffer = new(sourceEventBuffer.Message);
			InteractionBuffer = new(sourceEventBuffer.Interact);
			CompInteractBuffer = new(sourceEventBuffer.CompInteract);
			ReactAddBuffer = new(sourceEventBuffer.MsgAddReact);
			ContInteractBuffer = new(sourceEventBuffer.ContInteract);
		}
		public Task Run() => _sourceEventBuffer.EventLoops();
	}
	/// <summary>
	/// Single event pipe inliner.
	/// Forwards events to respective listeners, if not catched, forwards to base listeners.
	/// </summary>
	/// <typeparam name="TEvent"></typeparam>
	public class PerEventInline<TEvent> where TEvent : DiscordEventArgs
	{
		public static int DefaultOrder = 10;
		private Dictionary<int, List<Predictator<TEvent>>> _predictators;
		private SemaphoreSlim _lock;
		public string TypeName => GetType().FullName;
		public event AsyncEventHandler<DiscordClient, TEvent> OnFail;
		public PerEventInline(SingleEventBuffer<TEvent> buf)
		{
			_lock = new(1, 1);
			_predictators = new();
			buf.OnMessage += Check;
		}
		public async Task Add(int order, Predictator<TEvent> predictator)
		{
			await _lock.WaitAsync();
			if (!_predictators.ContainsKey(order))
				_predictators[order] = new();

			_predictators[order].Add(predictator);
			_lock.Release();
		}
		public Task Add(Predictator<TEvent> predictator) => Add(DefaultOrder, predictator);

		private async Task<IEnumerable<(int, Predictator<TEvent>)>> CheckEOL(IEnumerable<(int, Predictator<TEvent>)> input)
		{
			var rrr = Enumerable.Empty<(int, Predictator<TEvent>)>();
			foreach (var ch in input)
			{
				if (await ch.Item2.IsREOL())
					rrr = rrr.Append((ch.Item1, ch.Item2));
			}
			return rrr;
		}
		private async Task<IEnumerable<Predictator<TEvent>>> RunEvent(DiscordClient client, TEvent args, IEnumerable<Predictator<TEvent>> input)
		{
			var rrr = Enumerable.Empty<Predictator<TEvent>>();
			var handled = false;
			foreach (var chk in input)
			{
				if (handled && !chk.RunIfHandled || !await chk.IsFitting(client, args))
					continue;
				handled = true;
				rrr = rrr.Append(chk);
				args.Handled = true;
			}
			return rrr;
		}
		public async Task<bool> Check(DiscordClient client, TEvent args)
		{
			await _lock.WaitAsync();

			var itms = _predictators.SelectMany(x => x.Value.Select(y => (x.Key, y)));

			var itmsToDlt = await CheckEOL(itms);
			//Deletes and works!
			_ = itmsToDlt.Where(x => _predictators[x.Item1].Remove(x.Item2)
			 && _predictators[x.Item1].Count == 0 && _predictators.Remove(x.Item1)).ToArray();

			var toRun = await RunEvent(client, args, itms.Select(x => x.y));

			foreach (var itm in toRun)
				await itm.Handle(client, args);

			itmsToDlt = await CheckEOL(itms);
			//Deletes and works!
			_ = itmsToDlt.Where(x => _predictators[x.Item1].Remove(x.Item2)
			 && _predictators[x.Item1].Count == 0 && _predictators.Remove(x.Item1)).ToArray();

			_lock.Release();

			if (!toRun.Any() && OnFail != null)
				await OnFail(client, args);

			return !toRun.Any();
		}
	}
	public abstract class Predictator<TEvent> where TEvent : DiscordEventArgs
	{
		// Maybe create an ID for predictator, which client can receive buffered events
		public abstract Task<bool> IsFitting(DiscordClient client, TEvent args);
		public abstract bool RunIfHandled {
			get;
		}
		public abstract Task<bool> IsREOL();
		protected readonly DiscordEventProxy<(Predictator<TEvent>, TEvent)> _eventProxy;
		protected Predictator() => _eventProxy = new();
		public Task Handle(DiscordClient client, TEvent args) =>
			_eventProxy.Handle(client, (this, args));
		protected abstract Task CancelPredictator();
		public virtual Task<(DiscordClient, TEvent)> GetEvent(TimeSpan timeout, CancellationToken token)
		{
			return GetEvent(x => Task.Delay(timeout, token));
		}
		public virtual Task<(DiscordClient, TEvent)> GetEvent(CancellationToken token)
		{
			return GetEvent(x => Task.Delay(-1, token));
		}
		/// <summary>
		/// Get event in specific period of time
		/// </summary>
		/// <param name="timeout">The time period</param>
		/// <returns></returns>
		/// <exception cref="TimeoutException"></exception>
		public virtual async Task<(DiscordClient, TEvent)> GetEvent(TimeSpan timeout)
		{
			try
			{
				return await GetEvent(x => Task.Delay(timeout));
			}
			catch (TaskCanceledException e)
			{
				throw new TimeoutException($"Event awaiting for {timeout} has timed out!", e);
			}
		}
		/// <summary>
		/// Gets event
		/// </summary>
		/// <param name="cancelTask"></param>
		/// <returns></returns>
		/// <exception cref="TaskCanceledException"></exception>
		public async Task<(DiscordClient, TEvent)> GetEvent(
			Func<Task<(DiscordClient, (Predictator<TEvent>, TEvent))>, Task> genCancel)
		{
			var gettingData = _eventProxy.GetData();
			var cancelTask = genCancel(gettingData);
			var either = await Task.WhenAny(cancelTask, gettingData);

			if (either == cancelTask)
			{
				await CancelPredictator();
				throw new TaskCanceledException($"Event awaiting has been cancelled!");
			}

			var result = await gettingData;

			return (result.Item1, result.Item2.Item2);
		}

		public virtual async Task<(DiscordClient, TEvent)> GetEvent()
		{
			var result = await _eventProxy.GetData();

			return (result.Item1, result.Item2.Item2);
		}
	}
}