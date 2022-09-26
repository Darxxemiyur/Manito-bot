using Manito.Discord.Chat.DialogueNet;

using Name.Bayfaderix.Darxxemiyur.Node.Network;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.Discord.Orders
{
	public class AdminOrderDialogue : IDialogueNet
	{
		public NodeResultHandler StepResultHandler {
			get;
		}
		public NextNetworkInstruction GetStartingInstruction() => throw new NotImplementedException();
		public NextNetworkInstruction GetStartingInstruction(Object payload) => throw new NotImplementedException();
	}
}
