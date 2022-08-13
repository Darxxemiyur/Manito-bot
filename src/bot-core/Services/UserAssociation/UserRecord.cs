
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manito.Discord.UserAssociaton
{
    public class UserRecord
    {
        public ulong ID { get; set; }
        public ulong DiscordID { get; set; }
        [Column("SteamIDs", TypeName = "integer[]")]
        public List<ulong> SteamIDs { get; set; }
    }
}
