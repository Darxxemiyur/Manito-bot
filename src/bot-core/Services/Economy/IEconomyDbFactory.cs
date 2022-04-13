using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Manito.Discord.Db;

namespace Manito.Discord.Economy
{

    public interface IEconomyDbFactory : IDbFactory
    {

        IEconomyDb CreateEconomyDbContext();
        Task<IEconomyDb> CreateEconomyDbContextAsync();
    }

}
