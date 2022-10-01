using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Manito.Discord.Shop;

namespace Manito.Discord.Database
{

    public interface IMyDatabase : IDisposable
    {
        DbContextImplementation ImplementedContext { get; }
        /// <summary>
        /// Used to setup inner Db.
        /// </summary>
        /// <param name="factory">The factory to use</param>
        /// <returns></returns>
        void SetUpDatabase(IMyDbFactory factory);
        Task SetUpDatabaseAsync(IMyDbFactory factory);
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

}
