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

namespace Manito.Discord.Client
{
    /// <summary>
    /// Event inliner.
    /// Intercepts events that are being listened for from existing "sessions"
    /// Pushes non-interesting events further to the EventFilter's buffer
    /// </summary>
    public class EventInline
    {
        private PerEventInline<MessageCreateEventArgs> _messageBuffer;
        public event Func<DiscordClient, MessageCreateEventArgs, Task> OnMessage
        {
            add => _messageBuffer.OnFail += value;
            remove => _messageBuffer.OnFail -= value;
        }

        private PerEventInline<InteractionCreateEventArgs> _interactBuffer;
        public event Func<DiscordClient, InteractionCreateEventArgs, Task> OnInteraction
        {
            add => _interactBuffer.OnFail += value;
            remove => _interactBuffer.OnFail -= value;
        }

        private PerEventInline<ComponentInteractionCreateEventArgs> _compInteractBuffer;
        public event Func<DiscordClient, ComponentInteractionCreateEventArgs, Task> OnComponentInteraction
        {
            add => _compInteractBuffer.OnFail += value;
            remove => _compInteractBuffer.OnFail -= value;
        }
        private PerEventInline<MessageReactionAddEventArgs> _reactAddBuffer;
        public event Func<DiscordClient, MessageReactionAddEventArgs, Task> OnReactAdd
        {
            add => _reactAddBuffer.OnFail += value;
            remove => _reactAddBuffer.OnFail -= value;
        }

        private EventBuffer _sourceEventBuffer;
        public EventInline(EventBuffer sourceEventBuffer)
        {
            _sourceEventBuffer = sourceEventBuffer;
            _messageBuffer = new((x) => sourceEventBuffer.Message.OnMessage += x);
            _interactBuffer = new((x) => sourceEventBuffer.Interact.OnMessage += x);
            _compInteractBuffer = new((x) => sourceEventBuffer.CompInteract.OnMessage += x);
            _reactAddBuffer = new((x) => sourceEventBuffer.MsgAddReact.OnMessage += x);
        }
        public void Add(Predictator<MessageCreateEventArgs> predictate)
        {
            _messageBuffer.Add(predictate);
        }
        public void Add(Predictator<InteractionCreateEventArgs> predictate)
        {
            _interactBuffer.Add(predictate);
        }
        public void Add(Predictator<ComponentInteractionCreateEventArgs> predictate)
        {
            _compInteractBuffer.Add(predictate);
        }
        public void Add(Predictator<MessageReactionAddEventArgs> predictate)
        {
            _reactAddBuffer.Add(predictate);
        }
        public Task Run() => _sourceEventBuffer.EventLoops();
    }
    public class PerEventInline<TEvent> where TEvent : DiscordEventArgs
    {
        private List<Predictator<TEvent>> _predictators;
        public event Func<DiscordClient, TEvent, Task> OnFail;

        public PerEventInline(Action<Func<DiscordClient, TEvent, Task>> deleg)
        {
            _predictators = new();
            deleg(Check);
        }
        public void Add(Predictator<TEvent> predictator) => _predictators.Add(predictator);
        private async Task Check(DiscordClient client, TEvent args)
        {
            var toDelete = new List<Predictator<TEvent>>();
            var handled = false;

            foreach (var checker in _predictators)
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
                _predictators.Remove(item);

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
        protected Predictator()
        {
            _eventProxy = new();
        }
        public Task Handle(DiscordClient client, TEvent args) => _eventProxy
            .Handle(client, (this, args));
        public async Task<(DiscordClient, TEvent)> GetEvent(TimeSpan timeout)

        {
            var timeoutTask = Task.Delay(timeout);

            var gettingData = _eventProxy.GetData();

            var either = await Task.WhenAny(timeoutTask, gettingData);

            if (either == timeoutTask)
                return (null, null);

            var result = await gettingData;

            return (result.Item1, result.Item2.Item2);

        }

        public Task HasEvents() => _eventProxy.HasAny();
    }
}