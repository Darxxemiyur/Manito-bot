﻿using DSharpPlus.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.Discord.ChatNew
{
	public class DialogueMessageCatcher : EventCatchIdentifier<DiscordMessage>
	{
		public override Task<Boolean> AreWeDone()
		{
			throw new NotImplementedException();
		}
		public override Task<Boolean> DoWeCatchIt(DiscordMessage ev)
		{
			throw new NotImplementedException();
		}
	}
}