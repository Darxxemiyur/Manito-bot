using DSharpPlus.Entities;

using Manito.Discord.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.Discord.ChatNew
{
	/// <summary>
	/// Contract that sets interface with dialogue identifiers that
	/// identify dialogue where by user/channel/message ids, or component ids.
	/// </summary>
	public interface IDialogueIdentifier
	{
		/// <summary>
		/// Checks whether an interaction belongs to Dialogue session.
		/// </summary>
		/// <param name="interaction">The interaction being checked</param>
		/// <returns>true if it does, false if it doesn't</returns>
		bool DoesBelongToUs(InteractiveInteraction interaction);
	}
}
