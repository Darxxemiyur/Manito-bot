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
        public event Func<DiscordClient, TEvent, Task> OnFail;
        public PerEventInline(SingleEventBuffer<TEvent> buffer)
        {
            _predictators = new();
            buffer.OnMessage += Check;
        }
        public void Add(int order, Predictator<TEvent> predictator)
        {
            if (!_predictators.ContainsKey(order))
                _predictators[order] = new();

            _predictators[order].Add(predictator);
        }
        public void Add(Predictator<TEvent> predictator)
        {
            Add(DefaultOrder, predictator);
        }

        private async Task Check(DiscordClient client, TEvent args)
        {
            var handled = false;

            foreach (var checkerLine in _predictators)
            {
                var toDelete = new List<Predictator<TEvent>>();
                foreach (var checker in checkerLine.Value)
                {
                    if (await checker.IsREOL())
                    {
                        toDelete.Add(checker);
                    }
                    else if (await checker.IsFitting(args) && (!handled || checker.RunIfHandled))
                    {
                        handled = true;
                        await checker.Handle(client, args);
                        args.Handled = true;
                    }

                    if (await checker.IsREOL())
                        toDelete.Add(checker);
                }
                foreach (var item in toDelete)
                    checkerLine.Value.Remove(item);
            }


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
        private readonly DiscordEventProxy<(Predictator<TEvent>, TEvent)> _eventProxy;
        protected Predictator() => _eventProxy = new();
        public Task Handle(DiscordClient client, TEvent args) =>
            _eventProxy.Handle(client, (this, args));
        public async Task<(DiscordClient, TEvent)> GetEvent(TimeSpan timeout)

        {
            var timeoutTask = Task.Delay(timeout);

            var gettingData = _eventProxy.GetData();

            Console.WriteLine(4);
            var either = await Task.WhenAny(timeoutTask, gettingData);

            if (either == timeoutTask)
                return (null, null);

            var result = await gettingData;

            Console.WriteLine(result.ToString());

            return (result.Item1, result.Item2.Item2);

        }

        public Task HasEvents() => _eventProxy.HasAny();
    }
}