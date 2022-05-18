using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using Manito.Discord.Inventory;

namespace Manito.Services.Inventory
{
    public class PlayerInventory
    {
        private DiscordUser _player;
        private InventorySystem _inventorySystem;
        public PlayerInventory(InventorySystem inventorySystem, DiscordUser player)
        {
            _inventorySystem = inventorySystem;
            _player = player;
        }
        public IEnumerable<InventoryItem> GetInventoryItems()
        {
            return _inventorySystem.GetPlayerItems(_player);
        }
        public void AddItem(object item)
        {
            _inventorySystem.AddItem(_player, item);
        }
        public void RemoveItem(object item)
        {
            _inventorySystem.RemoveItem(_player, item);
        }
    }
}