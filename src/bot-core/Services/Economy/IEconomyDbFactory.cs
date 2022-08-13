using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Manito.Discord.Database;

namespace Manito.Discord.Economy
{

    public interface IEconomyDbFactory : IMyDbFactory
    {

        IEconomyDb CreateEconomyDbContext();
        Task<IEconomyDb> CreateEconomyDbContextAsync();
    }

}
