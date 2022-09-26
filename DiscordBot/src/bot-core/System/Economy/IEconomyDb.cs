using System;
using Microsoft.EntityFrameworkCore;

using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;

namespace Manito.Discord.Economy
{

    public interface IEconomyDb
    {
        DbSet<PlayerEconomyDeposit> PlayerEconomies { get; }
    }

}
