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

using Manito.Discord.Client;

namespace Manito.Discord.Chat
{
    public class ChannelHook
    {
        private ulong _channelId;
        private ulong _messageId;
        private EventInline _catcher;
        public ChannelHook(EventInline catcher)
        {
            _catcher = catcher;
        }
    }
}