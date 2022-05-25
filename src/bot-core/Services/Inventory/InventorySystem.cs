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
            for (var i = 0; i < 0; i++)
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
        public void AddItem(DiscordUser user, IItem item)
        {
            throw new NotImplementedException();
        }
        public void RemoveItem(DiscordUser user, IItem item)
        {
            throw new NotImplementedException();
        }
        public void ApplyItem(DiscordUser user, IItem item)
        {
            throw new NotImplementedException();
        }
        public bool HasItem(DiscordUser user, IItem item)
        {
            throw new NotImplementedException();
        }
        #region TEST
        public void TestAddItem(DiscordUser user, IItem item)
        {
            throw new NotImplementedException();
        }
        public void TestRemoveItem(DiscordUser user, IItem item)
        {
            _itemstest[user.Id].RemoveAll(x => x == item);
        }
        public void TestApplyItem(DiscordUser user, IItem item)
        {
            throw new NotImplementedException();
        }
        public bool TestHasItem(DiscordUser user, IItem item)
        {
            return _itemstest[user.Id].Any(x => x == item);
        }
        #endregion
    }
}