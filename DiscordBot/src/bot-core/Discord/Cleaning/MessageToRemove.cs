using System;

namespace Manito.Discord.Cleaning
{
	public class MessageToRemove
	{
		public MessageToRemove(ulong messageID, ulong channelID, DateTimeOffset expiration)
		{
			MessageID = messageID;
			ChannelID = channelID;
			Expiration = expiration;
		}

		public ulong MessageID {
			get; set;
		}

		public ulong ChannelID {
			get; set;
		}

		public DateTimeOffset Expiration {
			get; set;
		}

		public int TimesFailed {
			get; set;
		}
	}
}