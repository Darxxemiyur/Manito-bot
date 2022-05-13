using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Emzi0767.Utilities;
using System.Linq;
using System.Threading;

namespace Manito.Discord.Client
{
    /// <summary>
    /// Event inliner.
    /// Intercepts events that are being listened for from existing "sessions"
    /// Pushes non-interesting events further to the EventFilter's buffer
    /// </summary>
    public class EventInline
    {
        public PerEventInline<MessageCreateEventArgs> MessageBuffer { get; }
        public PerEventInline<InteractionCreateEventArgs> InteractionBuffer { get; }
        public PerEventInline<ComponentInteractionCreateEventArgs> CompInteractBuffer { get; }
        public PerEventInline<MessageReactionAddEventArgs> ReactAddBuffer { get; }

        private EventBuffer _sourceEventBuffer;
        public EventInline(EventBuffer sourceEventBuffer)
        {
            _sourceEventBuffer = sourceEventBuffer;
            MessageBuffer = new(sourceEventBuffer.Message);
            InteractionBuffer = new(sourceEventBuffer.Interact);
            CompInteractBuffer = new(sourceEventBuffer.CompInteract);
            ReactAddBuffer = new(sourceEventBuffer.MsgAddReact);
        }
        public Task Run() => _sourceEventBuffer.EventLoops();
    }
    public class PerEventInline<TEvent> where TEvent : DiscordEventArgs
    {
        public static int DefaultOrder = 10;
        private Dictionary<int, List<Predictator<TEvent>>> _predictators;
        private SemaphoreSlim _lock;
        public event Func<DiscordClient, TEvent, Task> OnFail;
        public PerEventInline(SingleEventBuffer<TEvent> buffer)
        {
            _lock = new(1, 1);
            _predictators = new();
            buffer.OnMessage += Check;
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

        private async Task Check(DiscordClient client, TEvent args)
        {
            await _lock.WaitAsync();
            var handled = false;
            List<(int, Predictator<TEvent>)> toDelete = new();
            foreach (var ch in _predictators.SelectMany(x => x.Value.Select(y => (x.Key, y))))
            {
                var chk = ch.y;
                if (await chk.IsREOL())
                {
                    toDelete.Add((ch.Key, chk));
                }
                else if (await chk.IsFitting(args) && (!handled || chk.RunIfHandled))
                {
                    handled = true;
                    await chk.Handle(client, args);
                    args.Handled = true;

                    if (await chk.IsREOL())
                    {
                        toDelete.Add((ch.Key, chk));
                    }

                }
            }

            toDelete.ForEach(x =>
            {
                _predictators[x.Item1].Remove(x.Item2);
                if (_predictators[x.Item1].Count <= 0)
                    _predictators.Remove(x.Item1);
            });
            _lock.Release();

            if (!handled && OnFail != null)
                await OnFail(client, args);
        }

    }
    public abstract class Predictator<TEvent> where TEvent : DiscordEventArgs
    {
        /// Maybe create an ID for predictator, which client can receive buffered events
        public abstract Task<bool> IsFitting(TEvent args);
        public abstract bool RunIfHandled { get; }
        public abstract Task<bool> IsREOL();
        protected readonly DiscordEventProxy<(Predictator<TEvent>, TEvent)> _eventProxy;
        protected Predictator() => _eventProxy = new();
        public Task Handle(DiscordClient client, TEvent args) =>
            _eventProxy.Handle(client, (this, args));
        public virtual async Task<(DiscordClient, TEvent)> GetEvent(TimeSpan timeout)

        {
            var timeoutTask = Task.Delay(timeout);

            var gettingData = _eventProxy.GetData();

            var either = await Task.WhenAny(timeoutTask, gettingData);

            if (either == timeoutTask)
                throw new TimeoutException($"Event awaiting for {timeout} has timed out!");

            var result = await gettingData;

            return (result.Item1, result.Item2.Item2);

        }
    }
}