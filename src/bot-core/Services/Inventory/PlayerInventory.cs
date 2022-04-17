using System.Collections.Generic;
using System.Linq;

namespace Manito.Services.Inventory
{
    public class PlayerInventory
    {
        private List<Item> _items;
        public PlayerInventory()
        {
            _items = new();
        }
        public IEnumerable<InventoryItem> GetInventoryItems()
        {
            return _items.Select(x => new InventoryItem(x));
        }
    }
}