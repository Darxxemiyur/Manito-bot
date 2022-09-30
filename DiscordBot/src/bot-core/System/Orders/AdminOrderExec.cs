using DisCatSharp.Entities;

using Manito.Discord.Chat.DialogueNet;
using Manito.Discord.ChatNew;

using Name.Bayfaderix.Darxxemiyur.Node.Network;

using System;
using System.Collections.Generic;
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
		} = Common.DefaultNodeResultHandler;
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
		private IEnumerator<OrderStep> _steps;
		public async Task ChangeOrder()
		{
			throw new NotImplementedException();
		}
		public async Task StopExecuting()
		{
			await Task.Run(() => Messanger.Cancel());
			return;
			//throw new NotImplementedException();
		}
		private async Task<NextNetworkInstruction> Decider(NetworkInstructionArgument arg)
		{
			_steps.MoveNext();
			var step = _steps.Current;
			if (step != null)
			{
				if (step.Type == OrderStepType.DoConfirmation)
					return new(DoConfirmation);
				if (step.Type == OrderStepType.DoCommand)
					return new(DoCommand);
			}

			return new(FetchNextStep);
		}
		private async Task<NextNetworkInstruction> DoCancellation(NetworkInstructionArgument arg)
		{
			await Session.RemoveMessage();
			return new();
		}
		private async Task<NextNetworkInstruction> DoConfirmation(NetworkInstructionArgument arg)
		{
			try
			{
				var success = new DiscordButtonComponent();
				var fail = new DiscordButtonComponent();

				await Session.SendMessage(new UniversalMessageBuilder().AddContent("Meme"));
				var intr = await Session.GetComponentInteraction(Messanger.Token);



				if (intr.CompareButton(success))
				{



				}
				if (intr.CompareButton(fail))
				{



				}
				throw new NotImplementedException();

			}
			catch (TaskCanceledException e)
			{
				return new(DoCancellation);
			}
		}
		private async Task<NextNetworkInstruction> DoCommand(NetworkInstructionArgument arg)
		{
			try
			{

				await Session.SendMessage(new UniversalMessageBuilder().AddContent("Meme"));
				var intr = await Session.GetComponentInteraction(Messanger.Token);



				if (intr.CompareButton())
				{

				}
				throw new NotImplementedException();
			}
			catch (TaskCanceledException e)
			{
				return new(DoCancellation);
			}
		}
		private async Task<NextNetworkInstruction> FetchNextStep(NetworkInstructionArgument arg)
		{
			throw new NotImplementedException();
		}
		public NextNetworkInstruction GetStartingInstruction() => new(Decider);
		public NextNetworkInstruction GetStartingInstruction(Object payload) => throw new NotImplementedException();
	}
}
