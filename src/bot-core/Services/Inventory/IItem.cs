using System;

namespace Manito.Services.Inventory
{
    public interface IItem
    {
        ulong Owner { get; set; }
        int Quantity
        {
            get;
            set;
        }
        int ItemType
        {
            get;
            set;
        }
    }
}