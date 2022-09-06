using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Manito.Discord.Shop;
using System.Threading;
using Manito.Discord.Economy;
using Manito.Discord.PermanentMessage;

namespace Manito.Discord.Database
{

    public class MyDatabase : IShopDb, IPermMessageDb, IEconomyDb, IMyDatabase
    {
        private bool disposedValue;

        public DbSet<ShopItem> ShopItems => ImplementedContext.ShopItems;
        public DbSet<PlayerEconomyDeposit> PlayerEconomies => ImplementedContext.PlayerEconomyDeposits;
        public DbSet<MessageWallTranslator> MessageWallTranslators => ImplementedContext.MessageWallTranslators;
        public DbSet<MessageWall> MessageWalls => ImplementedContext.MessageWalls;
        public DbSet<MessageWallLine> MessageWallLines => ImplementedContext.MessageWallLines;
        public DbContextImplementation ImplementedContext { get; private set; }


        public int SaveChanges() => ImplementedContext.SaveChanges();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
         => ImplementedContext.SaveChangesAsync(cancellationToken);

        public void SetUpDatabase(IMyDbFactory factory)
        {
            ImplementedContext = factory.OriginalFactory.CreateDbContext(null);
        }
        public Task SetUpDatabaseAsync(IMyDbFactory factory) =>
         Task.Run(() => SetUpDatabase(factory));

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MyDatabase()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }

}