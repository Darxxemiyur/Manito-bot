using DisCatSharp.Entities;

using Manito.Discord.Client;

namespace Manito.Discord.ChatNew
{
	/// <summary>
	/// Contract that sets interface with dialogue identifiers that identify dialogue where by
	/// user/channel/message ids, or component ids.
	/// </summary>
	public interface IDialogueIdentifier
	{
		/// <summary>
		/// Checks whether an interaction belongs to Dialogue session.
		/// </summary>
		/// <param name="interaction">The interaction being checked</param>
		/// <returns>true if it does, false if it doesn't</returns>
		bool DoesBelongToUs(InteractiveInteraction interaction);

		/// <summary>
		/// Describes how much it wants the interaction;
		/// </summary>
		/// <param name="interaction">The interaction to be checked</param>
		/// <returns>Want value</returns>
		int HowBadWants(InteractiveInteraction interaction);

		int HowBadIfWants(InteractiveInteraction interaction) => DoesBelongToUs(interaction) ? HowBadWants(interaction) : -1;

		/// <summary>
		/// Checks whether an interaction belongs to Dialogue session.
		/// </summary>
		/// <param name="interaction">The interaction being checked</param>
		/// <returns>true if it does, false if it doesn't</returns>
		bool DoesBelongToUs(DiscordMessage interaction);

		/// <summary>
		/// Describes how much it wants the interaction;
		/// </summary>
		/// <param name="interaction">The interaction to be checked</param>
		/// <returns>Want value</returns>
		int HowBadWants(DiscordMessage interaction);

		int HowBadIfWants(DiscordMessage interaction) => DoesBelongToUs(interaction) ? HowBadWants(interaction) : -1;

		ulong UserId {
			get;
		}

		ulong ChannelId {
			get;
		}

		ulong MessageId {
			get;
		}
	}
}