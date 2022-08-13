using System;
using Microsoft.EntityFrameworkCore;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using Manito.Discord.Database;
using System.Threading.Tasks;

namespace Manito.Discord.Shop
{

    public interface IShopDbFactory : IMyDbFactory
    {
        new IShopDb CreateMyDbContext();
        new Task<IShopDb> CreateMyDbContextAsync();
    }

}
