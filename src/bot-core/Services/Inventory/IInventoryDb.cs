using Manito.Discord.Db;
using Microsoft.EntityFrameworkCore;

namespace Manito.Discord.Inventory
{
    public interface IInventoryDb : IMyDb
    {
        DbSet<Item> ItemsDb
        {
            get;
        }
    }
}