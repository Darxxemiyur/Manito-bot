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
using Name.Bayfaderix.Darxxemiyur.Node.Network;
using Manito.Discord.Chat.DialogueNet;
using Name.Bayfaderix.Darxxemiyur.Node.Linkable;

namespace Manito.Discord.Client
{
    public class EventRoutingNet : BaseNodeSystem
    {
        private Func<Task> _toDo;
        public EventRoutingNet()
        {
            

        }

        public override Task LinkSystem() => _toDo();
    }
}