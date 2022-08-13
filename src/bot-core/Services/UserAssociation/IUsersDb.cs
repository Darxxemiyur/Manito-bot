using System;
using Microsoft.EntityFrameworkCore;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using Manito.Discord.Database;

namespace Manito.Discord.UserAssociaton
{

    public interface IUsersDb : IMyDatabase
    {
        DbSet<UserRecord> UserRecords { get; }
    }

}
