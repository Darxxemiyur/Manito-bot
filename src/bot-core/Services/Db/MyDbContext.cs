using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Manito.Discord.Shop;

namespace Manito.Discord.Db
{

    public class MyDbContext : DbContext, IShopDb, IMyDb
    {
        public DbSet<ShopItem> ShopItems => throw new NotImplementedException();
    }

}
