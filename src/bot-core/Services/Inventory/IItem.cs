using System;

namespace Manito.Services.Inventory
{
    public interface IItem
    {
        ulong Owner
        {
            get;
            set;
        }
        int Quantity
        {
            get;
            set;
        }
        string ItemType
        {
            get;
            set;
        }

    }
}