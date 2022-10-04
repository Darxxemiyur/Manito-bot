using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;
using Manito.Discord.Database;

namespace Manito.System.Economy
{

    public interface IEconomyDbFactory : IMyDbFactory
    {
        IEconomyDb CreateEconomyDbContext();
        Task<IEconomyDb> CreateEconomyDbContextAsync();
    }

}
