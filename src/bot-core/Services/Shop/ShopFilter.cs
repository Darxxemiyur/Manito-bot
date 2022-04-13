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

namespace Manito.Discord.Shop
{

    public class ShopFilter
    {
        private ShopService _shopService;
        private MyService _service;
        private List<DiscordApplicationCommand> _commandList;
        public ShopFilter(MyService service, EventBuffer eventBuffer)
        {
            _service = service;
            _shopService = new ShopService(service);
            _commandList = GetCommands().ToList();
            service.MyDiscordClient.AppCommands.Commands.Add("Shop", _commandList);
            eventBuffer.Interact.OnMessage += FilterMessage;
        }
        private IEnumerable<DiscordApplicationCommand> GetCommands()
        {
            yield return new DiscordApplicationCommand("shopping", "Начать шоппинг",
            defaultPermission: true);
        }
        public async Task FilterMessage(DiscordClient client, InteractionCreateEventArgs args)
        {
            if (_commandList.Any(x => args.Interaction.Data.Name.Contains(x.Name)))
            {
                await HandleAsCommand(args.Interaction);

                args.Handled = true;
            }
        }
        private async Task HandleAsCommand(DiscordInteraction args)
        {
            if (_shopService.CreateSession(args.User))
                await _service.MyDiscordClient.ExecutionThread.AddNew(
                    () => _shopService.GetSession(args.User).EnterMenu(args));
        }
    }

}