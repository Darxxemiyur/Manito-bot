using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.Discord.PermanentMessage
{
	public class ImportedMessage
	{
		public string Message {
			get; set;
		}
		public ulong UserId {
			get; set;
		}
		public ulong ChannelId {
			get; set;
		}
		public ulong MessageId {
			get; set;
		}
		public ulong GuildId {
			get; set;
		}
	}
}
