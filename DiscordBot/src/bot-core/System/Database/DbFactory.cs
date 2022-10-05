using Manito.Discord.Config;
using Manito.Discord.PermanentMessage;
using Manito.Discord.Shop;
using Manito.System.Economy;
using Manito.System.Logging;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using System.Threading.Tasks;

namespace Manito.Discord.Database
{
	public class MyDbFactory : IShopDbFactory, IEconomyDbFactory, IPermMessageDbFactory, ILoggingDBFactory, IMyDbFactory
	{
		private class DTDCF : IDesignTimeDbContextFactory<DbContextImplementation>
		{
			private DatabaseConfig _dbConfig;

			public DTDCF(DatabaseConfig dbConfig) => _dbConfig = dbConfig;

			public DbContextImplementation CreateDbContext(string[] args)
			{
				var optionsBuilder = new DbContextOptionsBuilder<DbContextImplementation>();

				optionsBuilder.UseNpgsql(_dbConfig.ConnectionString);

				optionsBuilder.EnableDetailedErrors();
				optionsBuilder.EnableSensitiveDataLogging();

				return new(optionsBuilder.Options);
			}
		}

		public IDesignTimeDbContextFactory<DbContextImplementation> OriginalFactory {
			get; private set;
		}

		public MyDbFactory(MyDomain domain, DatabaseConfig dbConfig)
		{
			OriginalFactory = new DTDCF(dbConfig);
		}

		public void SetUpFactory()
		{
		}

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
			return CreateMyDbContext();
		}

		async Task<IEconomyDb> IEconomyDbFactory.CreateEconomyDbContextAsync()
		{
			return await CreateMyDbContextAsync();
		}

		ILoggingDB ILoggingDBFactory.CreateLoggingDBContext()
		{
			return CreateMyDbContext();
		}

		async Task<ILoggingDB> ILoggingDBFactory.CreateLoggingDBContextAsync()
		{
			return await CreateMyDbContextAsync();
		}
	}
}