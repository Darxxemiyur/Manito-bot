using DSharpPlus.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.Discord.ChatNew
{
	public class DialogueIdentifier : IDialogueIdentifier
	{
		public Boolean DoesBelongToUs(DiscordInteraction interaction) => throw new NotImplementedException();
	}
}
