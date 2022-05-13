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
using Manito.Discord.Shop;
using Manito.Discord.Economy;


namespace Manito.Discord.Inventory
{
    public class InventoryFilter : IModule
    {
        public Task RunModule() => HandleLoop();
        private async Task HandleLoop()
        {
            while (true)
            {
                var data = await _queue.GetData();
                await HandleAsCommand(data.Item2.Interaction);
            }
        }
        private ShopService _shopService;
        private MyDomain _service;
        private List<DiscordApplicationCommand> _commandList;
        private DiscordEventProxy<InteractionCreateEventArgs> _queue;
        public InventoryFilter(MyDomain service, EventBuffer eventBuffer)
        {
            _service = service;
            _shopService = new ShopService(service);
            _commandList = GetCommands().ToList();
            service.MyDiscordClient.AppCommands.Commands.Add("Inventory", _commandList);
            _queue = new();
            eventBuffer.Interact.OnMessage += FilterMessage;
        }
        private IEnumerable<DiscordApplicationCommand> GetCommands()
        {
            yield return new DiscordApplicationCommand("inventory", "Открыть инвентарь",
            defaultPermission: true);
        }

        private async Task FilterMessage(DiscordClient client, InteractionCreateEventArgs args)
        {
            if (_commandList.Any(x => args.Interaction.Data.Name.Contains(x.Name)))
            {
                await _queue.Handle(client, args);
                args.Handled = true;
            }
        }
        private async Task HandleAsCommand(DiscordInteraction args)
        {
            
        }
    }

}