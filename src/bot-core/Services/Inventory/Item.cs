using System;

namespace Manito.Discord.Inventory
{
    public class Item : IItem
    {
        public ulong Id { get; set; }
        public ulong Owner { get; set; }
        public int Quantity { get; set; }
        public string ItemType { get; set; }
        public string Custom { get; set; }
    }
}