using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Manito.Discord.Inventory;
using Manito.Discord.Chat.DialogueNet;
using Manito.Discord.Client;
using Name.Bayfaderix.Darxxemiyur.Common;

namespace Manito.Discord.Inventory
{

    public class InventorySession : DialogueNetSession
    {
        private IInventorySystem _inventory;
        public IInventorySystem Inventory => _inventory;
        private InventoryController _controller;
        public PlayerInventory PInventory => Inventory.GetPlayerInventory(_user);
        public InventorySession(InteractiveInteraction iargs, MyDiscordClient client,
         IInventorySystem inventory, DiscordUser user, InventoryController controller)
          : base(iargs, client, user) => (_controller, _inventory) = (controller, inventory);

        public override async Task QuitSession()
        {

            await base.QuitSession();
        }
    }

}
