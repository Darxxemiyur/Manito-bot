using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using Manito.Discord.Inventory;

namespace Manito.Discord.Inventory
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
        public IEnumerable<InventoryItem> GetInventoryItems() =>
         _inventorySystem.GetPlayerItems(_player);
        public void AddItem(InventoryItem item)
        {
            _inventorySystem.AddItem(_player, item);
        }
        public void RemoveItem(InventoryItem item)
        {
            _inventorySystem.RemoveItem(_player, item);
        }
        public bool HasItem(InventoryItem item)
        {
            return _inventorySystem.HasItem(_player, item);
        }
        public void TestAddItem(InventoryItem item)
        {
            _inventorySystem.TestAddItem(_player, item);
        }
        public void TestRemoveItem(InventoryItem item)
        {
            _inventorySystem.TestRemoveItem(_player, item);
        }
        public bool TestHasItem(InventoryItem item)
        {
            return _inventorySystem.TestHasItem(_player, item);
        }
    }
}