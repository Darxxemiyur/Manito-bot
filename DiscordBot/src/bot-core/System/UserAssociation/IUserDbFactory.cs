using System;
using Microsoft.EntityFrameworkCore;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using Manito.Discord.Database;
using System.Threading.Tasks;

namespace Manito.Discord.UserAssociaton
{

    public interface IUserDbFactory : IMyDbFactory
    {
        new IUsersDb CreateMyDbContext();
        new Task<IUsersDb> CreateMyDbContextAsync();
    }

}
