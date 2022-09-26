using System;
using Microsoft.EntityFrameworkCore;

using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;

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
        public List<MessageWallLine> Msgs { get; set; }
        public MessageWall() => (Msgs, WallName) = (new(), "");
        public MessageWall(string name) => (Msgs, WallName) = (new(), name);
        public void SetName(string name) => WallName = name;
        public void AddMessage(MessageWallLine msg)
        {
            Msgs.Add(msg);
            Compact();
        }
        public void Compact() => Msgs = Msgs.Where(x => x.IsNull()).ToList();
        public IEnumerable<DiscordEmbedBuilder> GetEmbeds() => Msgs
            .Select(x => new DiscordEmbedBuilder().WithDescription(x));
    }
}
