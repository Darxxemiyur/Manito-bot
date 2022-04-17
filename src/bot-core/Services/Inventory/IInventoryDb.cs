using Manito.Discord.Db;
using Microsoft.EntityFrameworkCore;

namespace Manito.Services.Inventory
{
    public interface IInventoryDb : IMyDb
    {
        DbSet<Item> ItemsDb
        {
            get;
        }
    }
}