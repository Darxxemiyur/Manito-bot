using Manito.Discord.Chat.DialogueNet;
using Manito.Discord.ChatNew;

using Name.Bayfaderix.Darxxemiyur.Node.Network;

using System.Threading.Tasks;

namespace Manito.Discord.Shop
{
	public class BuyingStepsForError : IDialogueNet
	{
		private DialogueTabSession<ShopContext> _session;

		public BuyingStepsForError(DialogueTabSession<ShopContext> session) => _session = session;

		public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;

		public NextNetworkInstruction GetStartingInstruction(object payload) => GetStartingInstruction();

		public NextNetworkInstruction GetStartingInstruction() => new(SelectQuantity, NextNetworkActions.Continue);

		private async Task<NextNetworkInstruction> SelectQuantity(NetworkInstructionArgument args)
		{
			await _session.DoLaterReply();
			return new(null);
		}
	}
}