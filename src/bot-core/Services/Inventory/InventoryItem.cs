using System;

namespace Manito.Services.Inventory
{
    public class InventoryItem : IItem
    {
        private IItem _realItem;
        public InventoryItem(IItem realItem)
        {
            _realItem = realItem;
        }
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
        public int ItemType
        {
            get => _realItem.ItemType;
            set => _realItem.ItemType = value;
        }
    }
}