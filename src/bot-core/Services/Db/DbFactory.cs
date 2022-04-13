using System;
using System.Threading.Tasks;
using Manito.Discord.Shop;
using Microsoft.EntityFrameworkCore;

namespace Manito.Discord.Db
{

    public class DbFactory : IShopDbFactory, IDbFactory
    {
        public DbSet<ShopItem> ShopItems => throw new NotImplementedException();

        public MyDbContext CreateDbContext()
        {
            throw new NotImplementedException();
        }

        public Task<MyDbContext> CreateDbContextAsync()
        {
            throw new NotImplementedException();
        }

        public IMyDb CreateMyDbContext()
        {
            return CreateDbContext();
        }

        public async Task<IMyDb> CreateMyDbContextAsync()
        {
            return await CreateDbContextAsync();
        }
    }

}
