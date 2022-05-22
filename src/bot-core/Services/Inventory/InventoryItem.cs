using System;

namespace Manito.Discord.Inventory
{
    public class InventoryItem : IItem
    {
        private IItem _realItem;
        public InventoryItem(IItem realItem)
        {
            _realItem = realItem;
        }
        public ulong Id => _realItem.Id;
        public ulong Owner
        {
            get => _realItem.Owner;
            set => _realItem.Owner = value;
        }
        public int Quantity
        {
            get => _realItem.Quantity;
            set => _realItem.Quantity = value;
        }
        public string ItemType
        {
            get => _realItem.ItemType;
            set => _realItem.ItemType = value;
        }
        public static bool operator ==(InventoryItem item1, InventoryItem item2)
        {
            return item1.Id == item2.Id;

        }
        public static bool operator !=(InventoryItem item1, InventoryItem item2)
        {
            return !(item1 == item2);

        }
    }
}