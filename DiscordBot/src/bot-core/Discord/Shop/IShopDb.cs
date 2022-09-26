using System;
using Microsoft.EntityFrameworkCore;

using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;

using Manito.Discord.Database;

namespace Manito.Discord.Shop
{

    public interface IShopDb : IMyDatabase
    {
        DbSet<ShopItem> ShopItems { get; }
    }

}
