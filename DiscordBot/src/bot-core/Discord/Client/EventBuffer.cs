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

namespace Manito.Discord.Client
{

	public class EventBuffer
	{
		public SingleEventBuffer<MessageCreateEventArgs> Message;
		public SingleEventBuffer<InteractionCreateEventArgs> Interact;
		public SingleEventBuffer<ComponentInteractionCreateEventArgs> CompInteract;
		public SingleEventBuffer<ContextMenuInteractionCreateEventArgs> ContInteract;
		public SingleEventBuffer<MessageReactionAddEventArgs> MsgAddReact;

		public EventBuffer(DiscordClient client)
		{
			Message = new(x => client.MessageCreated += x,
			 x => client.MessageCreated -= x);
			Interact = new(x => client.InteractionCreated += x,
			 x => client.InteractionCreated -= x);
			CompInteract = new(x => client.ComponentInteractionCreated += x,
			 x => client.ComponentInteractionCreated -= x);
			MsgAddReact = new(x => client.MessageReactionAdded += x,
			 x => client.MessageReactionAdded -= x);
			ContInteract = new(x => client.ContextMenuInteractionCreated += x,
			 x => client.ContextMenuInteractionCreated -= x);
		}
		public EventBuffer(EventInline client)
		{
			Message = new(client.MessageBuffer);
			Interact = new(client.InteractionBuffer);
			CompInteract = new(client.CompInteractBuffer);
			MsgAddReact = new(client.ReactAddBuffer);
			ContInteract = new(client.ContInteractBuffer);

		}
		private IEnumerable<Task> GetLoops()
		{
			yield return Message.Loop();
			yield return Interact.Loop();
			yield return CompInteract.Loop();
			yield return MsgAddReact.Loop();
			yield return ContInteract.Loop();
		}
		public Task EventLoops() => Task.WhenAll(GetLoops());
	}

	public class SingleEventBuffer<TEvent> where TEvent : DiscordEventArgs
	{
		private DiscordEventProxy<TEvent> _eventBuffer;
		private Action<AsyncEventHandler<DiscordClient, TEvent>> _unlinker;
		public event Func<DiscordClient, TEvent, Task> OnMessage;
		private void CreateEventBuffer()
		{
			_eventBuffer = new();
		}
		public SingleEventBuffer(Action<AsyncEventHandler<DiscordClient, TEvent>> linker,
		 Action<AsyncEventHandler<DiscordClient, TEvent>> unlinker)
		{
			CreateEventBuffer();
			linker(_eventBuffer.Handle);
			_unlinker = unlinker;
		}
		public SingleEventBuffer(PerEventInline<TEvent> linker)
		{
			CreateEventBuffer();
			linker.OnFail += _eventBuffer.Handle;
		}
		~SingleEventBuffer()
		{
			_unlinker(_eventBuffer.Handle);
		}
		public async Task Loop()
		{
			while (true)
			{
				var data = await _eventBuffer.GetData();
				if (OnMessage != null)
					await OnMessage(data.Item1, data.Item2);
			}
		}
	}

}
