using System;
using Microsoft.EntityFrameworkCore;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using Manito.Discord.Db;

namespace Manito.Discord.Shop
{

    public interface IShopDbFactory : IDbFactory
    {
        DbSet<ShopItem> ShopItems { get; }
    }

}
