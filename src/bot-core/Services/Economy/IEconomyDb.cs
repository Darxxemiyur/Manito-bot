using System;
using Microsoft.EntityFrameworkCore;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace Manito.Discord.Economy
{

    public interface IEconomyDb
    {
        DbSet<PlayerEconomyDeposit> PlayerEconomies { get; }
    }

}
