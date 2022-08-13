using System;
using Microsoft.EntityFrameworkCore;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using Manito.Discord.Database;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Cyriller;

namespace Manito.Discord.PermanentMessage
{

    public class MessageWallLine
    {
        public ulong ID { get; set; }
        public MessageWall MessageWall { get; set; }
        public string WallLine { get; private set; }
        public MessageWallLine() {}
        public MessageWallLine(string line) => WallLine = line;
        public void SetLine(string line) => WallLine = line;
        public bool IsNull() => string.IsNullOrWhiteSpace(WallLine);
        public static implicit operator string(MessageWallLine mwl) => mwl.WallLine;
    }
}
