using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using Manito.Discord.Client;


namespace Manito.Discord.Inventory
{
    public class InventoryDialogue
    {
        private PlayerInventory _inventory;
        public InventoryDialogue(PlayerInventory inventory) => _inventory = inventory;

        

    }
}