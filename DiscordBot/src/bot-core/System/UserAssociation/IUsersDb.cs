using System;
using Microsoft.EntityFrameworkCore;

using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;

using Manito.Discord.Database;

namespace Manito.System.UserAssociaton
{

    public interface IUsersDb : IMyDatabase
    {
        DbSet<UserRecord> UserRecords { get; }
    }

}
