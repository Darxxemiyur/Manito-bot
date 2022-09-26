using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.EventArgs;
using Manito.Discord.Client;
using Name.Bayfaderix.Darxxemiyur.Common;


namespace Manito.Discord.Inventory
{
    public class InventoryDialogue
    {
        private PlayerInventory _inventory;
        public InventoryDialogue(PlayerInventory inventory) => _inventory = inventory;

        

    }
}