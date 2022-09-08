using DSharpPlus.EventArgs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.Discord.ChatNew
{
	/// <summary>
	/// Used to define what kind of event message to catch and how to act later.
	/// </summary>
	public abstract class EventCatchIdentifier<T> where T : DiscordEventArgs
	{
		public abstract Task<bool> DoWeCatchIt(T ev);
		public abstract Task<bool> AreWeDone();
	}
}
