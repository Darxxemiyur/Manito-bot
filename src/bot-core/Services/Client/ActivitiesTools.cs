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
using DSharpPlus.Interactivity.EventHandling;

namespace Manito.Discord.Client
{
    public class ActivitiesTools
    {
        private EventInline _evInline;
        public ActivitiesTools(EventInline evInline)
        {
            _evInline = evInline;
        }

        public Task<MessageReactionAddEventArgs> WaitForReaction(DiscordMessage message)
        {
            return WaitFor((MessageReactionAddEventArgs x) =>
            x.Message.Id == message.Id, TimeSpan.FromMinutes(10));
        }
        public Task<MessageCreateEventArgs> WaitForMessage(
            Func<MessageCreateEventArgs, bool> checker)
        {
            return WaitForMessage(checker, TimeSpan.FromMinutes(10));
        }
        public Task<MessageCreateEventArgs> WaitForMessage(
            Func<MessageCreateEventArgs, bool> checker, TimeSpan timeout)
        {
            return WaitFor(checker, timeout);
        }
        public Task<ComponentInteractionCreateEventArgs> WaitForComponentInteraction(
            DiscordMessage message)
        {
            return WaitForComponentInteraction(
                (ComponentInteractionCreateEventArgs x) => x.Message.Id == message.Id);
        }
        public Task<ComponentInteractionCreateEventArgs> WaitForComponentInteraction(
            Func<ComponentInteractionCreateEventArgs, bool> checker)
        {
            return WaitForComponentInteraction(checker, TimeSpan.FromMinutes(10));
        }
        public async Task<ComponentInteractionCreateEventArgs> WaitForComponentInteraction(
            Func<ComponentInteractionCreateEventArgs, bool> checker, TimeSpan timeout)
        {
            var catcher = new SingleEventCatcher<ComponentInteractionCreateEventArgs>(checker);
            _evInline.CompInteractBuffer.Add(catcher);
            var evnv = await catcher.GetEvent(timeout);

            return evnv.Item2;
        }
        public async Task<MessageReactionAddEventArgs> WaitFor(
            Func<MessageReactionAddEventArgs, bool> pred, TimeSpan timeout)
        {
            var catcher = new SingleEventCatcher<MessageReactionAddEventArgs>(pred);
            _evInline.ReactAddBuffer.Add(catcher);
            Console.WriteLine(13);
            var evnv = await catcher.GetEvent(timeout);

            return evnv.Item2;
        }
        public async Task<MessageCreateEventArgs> WaitFor(
            Func<MessageCreateEventArgs, bool> pred, TimeSpan timeout)
        {
            var catcher = new SingleEventCatcher<MessageCreateEventArgs>(pred);
            _evInline.MessageBuffer.Add(catcher);
            var evnv = await catcher.GetEvent(timeout);

            return evnv.Item2;
        }

    }
}