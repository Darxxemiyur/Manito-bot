using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using Manito.Discord.Inventory;

namespace Manito.Services.Inventory
{
    public class PlayerInventory
    {
        private List<Item> _items;
        private DiscordUser _player;
        private InventorySystem _inventorySystem;
        public PlayerInventory(InventorySystem inventorySystem, DiscordUser player)
        {
            _items = new();
            _player = player;
        }
        public IEnumerable<InventoryItem> GetInventoryItems()
        {
            return _items.Select(x => new InventoryItem(x));
        }
    }
}