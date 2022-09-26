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
using DisCatSharp.Interactivity.EventHandling;
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