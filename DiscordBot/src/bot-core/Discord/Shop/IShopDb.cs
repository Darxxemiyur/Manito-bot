using System;
using Microsoft.EntityFrameworkCore;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using Manito.Discord.Database;

namespace Manito.Discord.Shop
{

    public interface IShopDb : IMyDatabase
    {
        DbSet<ShopItem> ShopItems { get; }
    }

}
