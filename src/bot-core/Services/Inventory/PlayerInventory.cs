using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Manito.Discord.Inventory;

namespace Manito.Discord.Inventory
{
    public class PlayerInventory
    {
        private DiscordUser _player;
        private IInventorySystem _inventorySystem;
        public DiscordUser Player => _player;
        public PlayerInventory(IInventorySystem inventorySystem, DiscordUser player)
        {
            _inventorySystem = inventorySystem;
            _player = player;
        }
        public IEnumerable<InventoryItem> GetInventoryItems() =>
         _inventorySystem.GetPlayerItems(_player);
        public Task AddItem(Func<Item, Item> item) => _inventorySystem.AddItem(_player, item);
        public Task AddItem(Action<Item> item) => _inventorySystem.AddItem(_player,
         x => { item(x); return x; });
        public Task RemoveItem(InventoryItem item) => _inventorySystem.RemoveItem(_player, item);
        public Task<bool> HasItem(InventoryItem item) => _inventorySystem.HasItem(_player, item);
    }
}