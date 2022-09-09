using DSharpPlus.Entities;

using Manito.Discord.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.Discord.ChatNew
{
	public class DialogueIdentifier : IDialogueIdentifier
	{
		public DialogueIdentifier(DiscordMessage message)
		{
			_chId = message.ChannelId;
			_msId = message.Id;
			_usId = message.Author.Id;
		}
		private ulong _usId;
		private ulong _chId;
		private ulong _msId;
		public Boolean DoesBelongToUs(InteractiveInteraction interaction)
		{
			var mid = interaction.Message.Id;
			var cid = interaction.Message.ChannelId;
			var uid = interaction.Interaction.User.Id;

			return _chId == cid && mid == _msId && uid == _usId;
		}
		public Int32 HowBadWants(InteractiveInteraction interaction)
		{
			return 10;
		}
	}
}
