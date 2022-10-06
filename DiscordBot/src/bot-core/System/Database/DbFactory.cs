using Manito.Discord.Client;
using Manito.Discord.Config;
using Manito.Discord.PermanentMessage;
using Manito.Discord.Shop;
using Manito.System.Economy;
using Manito.System.Logging;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

using Name.Bayfaderix.Darxxemiyur.Common;

using System.Threading.Tasks;

namespace Manito.Discord.Database
{
	public class MyDbFactory : IShopDbFactory, IEconomyDbFactory, IPermMessageDbFactory, ILoggingDBFactory, IMyDbFactory, IModule
	{
		private class DTDCF : IDbContextFactory<DbContextImplementation>, IDesignTimeDbContextFactory<DbContextImplementation>
		{
			private readonly DatabaseConfig _dbConfig;
			public DTDCF()
			{
			}
			public DTDCF(DatabaseConfig dbConfig) => _dbConfig = dbConfig;

			public DbContextImplementation CreateDbContext(string[] args) => CreateDbContext();
			public DbContextImplementation CreateDbContext()
			{

				var optionsBuilder = new DbContextOptionsBuilder<DbContextImplementation>();

				optionsBuilder.UseNpgsql(_dbConfig?.ConnectionString ?? "Data Source=blog.db");

				optionsBuilder.EnableDetailedErrors();
				optionsBuilder.EnableSensitiveDataLogging();

				return new(optionsBuilder.Options);
			}
		}

		public IDesignTimeDbContextFactory<DbContextImplementation> OriginalFactory {
			get; private set;
		}
		private AsyncLocker _lock;
		public MyDbFactory(MyDomain domain, DatabaseConfig dbConfig)
		{
			OriginalFactory = new DTDCF(dbConfig);
			_lock = new();
		}

		public MyDatabase CreateMyDbContext()
		{
			using var _ = _lock.BlockLock();
			var db = new MyDatabase();
			db.SetUpDatabase(this);
			return db;
		}

		public async Task<MyDatabase> CreateMyDbContextAsync()
		{
			await using var _ = await _lock.BlockAsyncLock();
			return await InternalCreateAsync();
		}

		private async Task<MyDatabase> InternalCreateAsync()
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

		public async Task RunModule()
		{
#if !DEBUG
			await using var _ = await _lock.BlockAsyncLock();
			await using var db = await InternalCreateAsync();

			await db.ImplementedContext.Database.MigrateAsync();
#endif
		}
	}
}