using System;
using Microsoft.EntityFrameworkCore;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using Manito.Discord.Database;

namespace Manito.Discord.PermanentMessage
{

    public interface IPermMessageDb : IMyDatabase
    {
        DbSet<MessageWallTranslator> MessageWallTranslators { get; }
        DbSet<MessageWall> MessageWalls { get; }
        DbSet<MessageWallLine> MessageWallLines { get; }
    }

}
