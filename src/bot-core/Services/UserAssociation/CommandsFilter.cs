

using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Manito.Discord.Client;

namespace Manito.Discord.UserAssociaton
{
    public class CommandsFilter
    {
        private MyDomain _service;
        private DiscordEventProxy<InteractionCreateEventArgs> _queue;
        private UserAssociatonCommands _commands;
        private List<DiscordApplicationCommand> _commandList;
        public CommandsFilter(MyDomain service, EventBuffer eventBuffer)
        {
            _service = service;
            _commands = new();
            _commandList = _commands.GetCommands();
            service.MyDiscordClient.AppCommands.Add("UserAssoc", _commandList);
            _queue = new();
            eventBuffer.Interact.OnMessage += FilterMessage;
        }
        public async Task FilterMessage(DiscordClient client, InteractionCreateEventArgs args)
        {
            var res = _commands.Search(args.Interaction);
            if (res == null)
                return;

            await res(args.Interaction);
            args.Handled = true;
        }
    }
}