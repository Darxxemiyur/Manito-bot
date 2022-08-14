using System;
using Microsoft.EntityFrameworkCore;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using Manito.Discord.Database;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Cyriller;
using System.ComponentModel.DataAnnotations;

namespace Manito.Discord.PermanentMessage
{
    /// <summary>
    /// SetOf Messages
    /// </summary>
    public class MessageWall
    {
        public ulong ID { get; set; }
        public string WallName { get; set; }
        private List<MessageWallLine> _msgs;
        public List<MessageWallLine> Msgs => _msgs;
        public MessageWall() => (_msgs, WallName) = (new(), "");
        public MessageWall(string name) => (_msgs, WallName) = (new(), name);
        public void SetName(string name) => WallName = name;
        public void AddMessage(MessageWallLine msg)
        {
            _msgs.Add(msg);
            Compact();
        }

        public void Compact() => _msgs = _msgs.Where(x => x.IsNull()).ToList();
        public IEnumerable<DiscordEmbedBuilder> GetEmbeds()
        {
            return _msgs.Select(x => new DiscordEmbedBuilder().WithDescription(x));
        }
    }
}
