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

    public class EventBuffer
    {
        public SingleEventBuffer<MessageCreateEventArgs> Message;
        public SingleEventBuffer<InteractionCreateEventArgs> Interact;
        public SingleEventBuffer<ComponentInteractionCreateEventArgs> CompInteract;
        public SingleEventBuffer<MessageReactionAddEventArgs> MsgAddReact;

        public EventBuffer(DiscordClient client)
        {
            Message = new(x => client.MessageCreated += x);
            Interact = new(x => client.InteractionCreated += x);
            CompInteract = new(x => client.ComponentInteractionCreated += x);
            MsgAddReact = new(x => client.MessageReactionAdded += x);
        }
        public EventBuffer(EventInline client)
        {
            Message = new(x => client.OnMessage += x);
            Interact = new(x => client.OnInteraction += x);
            CompInteract = new(x => client.OnComponentInteraction += x);
            MsgAddReact = new(x => client.OnReactAdd += x);
        }
        private IEnumerable<Task> GetLoops()
        {
            yield return Message.Loop();
            yield return Interact.Loop();
            yield return CompInteract.Loop();
            yield return MsgAddReact.Loop();
        }
        public Task EventLoops() => Task.WhenAll(GetLoops());
    }

    public class SingleEventBuffer<TEvent> where TEvent : DiscordEventArgs
    {
        private DiscordEventProxy<TEvent> _eventBuffer;
        public event Func<DiscordClient, TEvent, Task> OnMessage;
        private void CreateEventBuffer()
        {
            _eventBuffer = new();
        }
        public SingleEventBuffer(Action<AsyncEventHandler<DiscordClient, TEvent>> linker)
        {
            CreateEventBuffer();
            linker(_eventBuffer.Handle);
        }
        public SingleEventBuffer(Action<Func<DiscordClient, TEvent, Task>> linker)
        {
            CreateEventBuffer();
            linker(_eventBuffer.Handle);
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
