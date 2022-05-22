using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using Manito.Discord.Inventory;

namespace Manito.Discord.Inventory
{

    public class InventorySystem
    {
        private List<Item> _items;
        private Dictionary<ulong, List<Item>> _itemstest;
        public InventorySystem()
        {
            _items = new();
            _itemstest = new();
        }
        private IEnumerable<Item> GenerateNewUserItems(ulong id)
        {
            for (var i = 0; i < 24 * 4; i++)
            {
                yield return new Item() { Id = (ulong)i, Owner = id, ItemType = $"Bonus{i + 1}" };
            }
        }
        public IEnumerable<InventoryItem> GetPlayerItems(DiscordUser user)
        {
            if (!_itemstest.ContainsKey(user.Id))
                _itemstest.Add(user.Id, GenerateNewUserItems(user.Id).ToList());

            return _itemstest.FirstOrDefault(x => x.Key == user.Id).Value
                .Select(x => new InventoryItem(x));
        }
        public void AddItem(DiscordUser user, object item)
        {
            throw new NotImplementedException();
        }
        public void TestAddItem(DiscordUser user, object item)
        {
            throw new NotImplementedException();
        }
        public void RemoveItem(DiscordUser user, object item)
        {
            throw new NotImplementedException();
        }
        public void TestRemoveItem(DiscordUser user, InventoryItem item)
        {
            _itemstest[user.Id].Remove(_itemstest[user.Id].First(x => new InventoryItem(x) == item));
        }
        public void ApplyItem(DiscordUser user, object item)
        {
            throw new NotImplementedException();
        }
        public void TestApplyItem(DiscordUser user, object item)
        {
            throw new NotImplementedException();
        }
    }
}