using Manito.Discord.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.Discord.ChatNew
{
	/// <summary>
	/// Used to hold and describe session and its information.
	/// </summary>
	public class SessionInformation
	{
		public IDialogueIdentifier Identifier {
			get; private set;
		}
		public event EventHandler<IDialogueIdentifier> OnIdentifierUpdate;
		public void UpdateId(IDialogueIdentifier id) => OnIdentifierUpdate?.Invoke(this, Identifier = id);

		public InteractiveInteraction Interaction {
			get; private set;
		}
		public event EventHandler<InteractiveInteraction> OnInteractionUpdate;
		public void UpdateInteraction(InteractiveInteraction interaction) => OnInteractionUpdate?.Invoke(this, Interaction = interaction);
		public MyDiscordClient Client {
			get; private set;
		}
		public SessionInformation(MyDiscordClient client) => Client = client;
	}
}
