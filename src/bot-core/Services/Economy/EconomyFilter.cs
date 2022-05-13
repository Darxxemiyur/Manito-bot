using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using DSharpPlus.SlashCommands.Attributes;

using Manito.Discord.Client;
using Manito.Discord.Economy;

namespace Manito.Discord.Economy
{

    public class EconomyFilter : IModule
    {

        public Task RunModule() => HandleLoop();
        private async Task HandleLoop()
        {
            while (true)
            {
                var data = await _queue.GetData();
                await FilterMessage(data.Item1, data.Item2);
            }
        }
        private EconomyCommands _commands;
        private List<DiscordApplicationCommand> _commandList;
        private DiscordEventProxy<InteractionCreateEventArgs> _queue;
        public EconomyFilter(MyDomain service, EventBuffer eventBuffer)
        {
            _commands = new EconomyCommands(service.Economy);
            _commandList = _commands.GetCommands().ToList();
            service.MyDiscordClient.AppCommands.Commands.Add("Economy", _commandList);
            _queue = new();
            eventBuffer.Interact.OnMessage += _queue.Handle;
        }
        public async Task FilterMessage(DiscordClient client, InteractionCreateEventArgs args)
        {
            var res = _commands.Search(args.Interaction);
            if (res != null)
            {
                await res(args.Interaction);
                args.Handled = true;
            }
        }
    }

}