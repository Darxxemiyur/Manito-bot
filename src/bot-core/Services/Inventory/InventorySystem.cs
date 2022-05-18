using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using Manito.Services.Inventory;

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
            yield return new Item() { Owner = id };
            yield return new Item() { Owner = id };
            yield return new Item() { Owner = id };
            yield return new Item() { Owner = id };
            yield return new Item() { Owner = id };
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
        public void RemoveItem(DiscordUser user, object item)
        {
            throw new NotImplementedException();
        }
        public void ApplyItem(DiscordUser user, object item)
        {
            throw new NotImplementedException();
        }
    }
}