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

    public class EconomyFilter
    {
        private EconomyCommands _commands;
        private MyService _service;
        private List<DiscordApplicationCommand> _commandList;
        public EconomyFilter(MyService service, EventBuffer eventBuffer)
        {
            _service = service;
            _commands = new EconomyCommands();
            _commandList = _commands.GetCommands().Select(x => x.Item1).ToList();
            service.MyDiscordClient.AppCommands.Commands.Add("Economy", _commandList);
            eventBuffer.Interact.OnMessage += FilterMessage;
        }
        public async Task FilterMessage(DiscordClient client, InteractionCreateEventArgs args)
        {
            if (_commandList.Any(x => args.Interaction.Data.Name.Contains(x.Name)))
            {
                await _commands.HandleCommands(args.Interaction);

                args.Handled = true;
            }
        }
    }

}