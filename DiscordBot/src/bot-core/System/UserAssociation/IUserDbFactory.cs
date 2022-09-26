using System;
using Microsoft.EntityFrameworkCore;

using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;

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
