using System;
using Microsoft.EntityFrameworkCore;

using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;

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
