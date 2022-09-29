using Manito.Discord.Chat.DialogueNet;
using Manito.Discord.ChatNew;

using Name.Bayfaderix.Darxxemiyur.Common;
using Name.Bayfaderix.Darxxemiyur.Node.Network;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Manito.Discord.Orders
{
	public enum OrderStepType
	{
		DoConfirmation,
		DoCommand,
	}
	public class AdminOrderExec : IDialogueNet
	{
		public NodeResultHandler StepResultHandler {
			get;
		}
		public readonly CancellationTokenSource Messanger;
		public readonly UniversalSession Session;
		public AdminOrderExec(UniversalSession session)
		{
			Messanger = new();
			Session = session;
		}

		public Order ExOrder {
			get; set;
		}
		public async Task ChangeOrder()
		{
			throw new NotImplementedException();
		}
		public async Task StopExecuting()
		{
			return;
			//throw new NotImplementedException();
		}
		private async Task<NextNetworkInstruction> Decider(NetworkInstructionArgument arg)
		{
			try
			{
				var newComponent = await Session.GetComponentInteraction(Messanger.Token);




			}
			catch (TaskCanceledException e)
			{
				return new(DoCancellation);
			}
			throw new NotImplementedException();
		}
		private async Task<NextNetworkInstruction> DoCancellation(NetworkInstructionArgument arg)
		{
			throw new NotImplementedException();
		}
		private async Task<NextNetworkInstruction> DoConfirmation(NetworkInstructionArgument arg)
		{
			throw new NotImplementedException();
		}
		private async Task<NextNetworkInstruction> DoCommand(NetworkInstructionArgument arg)
		{
			throw new NotImplementedException();
		}
		private async Task<NextNetworkInstruction> FetchNextStep(NetworkInstructionArgument arg)
		{
			throw new NotImplementedException();
		}
		public NextNetworkInstruction GetStartingInstruction() => throw new NotImplementedException();
		public NextNetworkInstruction GetStartingInstruction(Object payload) => throw new NotImplementedException();
	}
}
