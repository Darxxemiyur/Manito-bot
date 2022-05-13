using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Manito.Discord.Db
{
    /// <summary>
    /// Basic interface for DbFactory
    /// </summary>
    public interface IDbFactory : IDbContextFactory<MyDbContext>
    {
        MyDbContext CreateDbContext();
        Task<MyDbContext> CreateDbContextAsync();
        IMyDb CreateMyDbContext();
        Task<IMyDb> CreateMyDbContextAsync();
    }
    /// <inheritdoc/>
    public interface IMyDbFactory : IDbFactory
    {

    }
}
