using System;

namespace Manito.Services.Inventory
{
    public class Item : IItem
    {
        public ulong Id { get; set; }
        public ulong Owner { get; set; }
        public int Quantity { get; set; }
        public int ItemType { get; set; }
        public string Custom { get; set; }
    }
}