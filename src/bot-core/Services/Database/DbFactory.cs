using System;
using System.Data.Common;
using System.Threading.Tasks;
using Manito.Discord.Config;
using Manito.Discord.Economy;
using Manito.Discord.PermanentMessage;
using Manito.Discord.Shop;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Internal;
using Npgsql;

namespace Manito.Discord.Database
{

    public class MyDbFactory : IShopDbFactory, IEconomyDbFactory, IPermMessageDbFactory, IMyDbFactory
    {
        private class DTDCF : IDesignTimeDbContextFactory<DbContextImplementation>
        {
            private DatabaseConfig _dbConfig;
            public DTDCF(DatabaseConfig dbConfig) => _dbConfig = dbConfig;
            public DbContextImplementation CreateDbContext(string[] args)
            {
                var optionsBuilder = new DbContextOptionsBuilder<DbContextImplementation>();
                //optionsBuilder.UseSqlite(String.Format("Data Source={0}", Configuration.Config.DBPath));

                optionsBuilder.UseNpgsql(_dbConfig.ConnectionString);

                return new(optionsBuilder.Options);
            }
        }
        public IDesignTimeDbContextFactory<DbContextImplementation> OriginalFactory { get; private set; }
        public MyDbFactory(MyDomain domain, DatabaseConfig dbConfig)
        {
            OriginalFactory = new DTDCF(dbConfig);
        }
        public void SetUpFactory() { }
        public Task SetUpFactoryAsync() => Task.CompletedTask;

        public MyDatabase CreateMyDbContext()
        {
            var db = new MyDatabase();
            db.SetUpDatabase(this);
            return db;
        }

        public async Task<MyDatabase> CreateMyDbContextAsync()
        {
            var db = new MyDatabase();
            await db.SetUpDatabaseAsync(this);
            return db;
        }
        IShopDb IShopDbFactory.CreateMyDbContext()
        {
            return CreateMyDbContext();
        }
        async Task<IShopDb> IShopDbFactory.CreateMyDbContextAsync()
        {
            return await CreateMyDbContextAsync();
        }
        IMyDatabase IMyDbFactory.CreateMyDbContext()
        {
            return CreateMyDbContext();
        }

        async Task<IMyDatabase> IMyDbFactory.CreateMyDbContextAsync()
        {
            return await CreateMyDbContextAsync();
        }

        IPermMessageDb IPermMessageDbFactory.CreateMyDbContext()
        {
            return CreateMyDbContext();
        }

        async Task<IPermMessageDb> IPermMessageDbFactory.CreateMyDbContextAsync()
        {
            return await CreateMyDbContextAsync();
        }

        IEconomyDb IEconomyDbFactory.CreateEconomyDbContext()
        {
            throw new NotImplementedException();
        }

        Task<IEconomyDb> IEconomyDbFactory.CreateEconomyDbContextAsync()
        {
            throw new NotImplementedException();
        }
    }
}
