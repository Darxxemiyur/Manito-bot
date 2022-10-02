using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Manito.Discord.Chat.DialogueNet;
using Manito.Discord.ChatNew;

using Name.Bayfaderix.Darxxemiyur.Node.Network;
using Name.Bayfaderix.Darxxemiyur.Common;

using System;
using System.Threading.Tasks;

namespace Manito.Discord.Orders
{
	public class AdminOrderControl : IDialogueNet
	{
		public NodeResultHandler StepResultHandler {
			get;
		} = Common.DefaultNodeResultHandler;
		private DialogueTabSession<AdminOrderContext> _session;
		private MyDomain Domain => _session.Client.Domain;
		private AdminOrderExec _execSession;
		private DiscordButtonComponent _beginButton;
		private DiscordButtonComponent _changeButton;
		private DiscordButtonComponent _endButton;
		private PoolTaskEventProxy _pool;
		public AdminOrderControl(DialogueTabSession<AdminOrderContext> session, PoolTaskEventProxy pool)
		{
			_pool = pool;
			_session = session;
			_beginButton = new(ButtonStyle.Success, "beginworking", "Начать работу.");
			_changeButton = new(ButtonStyle.Primary, "changeorder", "Сменить заказ.", true);
			_endButton = new(ButtonStyle.Danger, "endworking", "Закончить работу.", true);
		}
		private async Task<NextNetworkInstruction> BeginOrderExecution(NetworkInstructionArgument arg)
		{
			var (channel, id) = ((DiscordChannel, DiscordUser))arg.Payload;
			_execSession = new(_pool, new(new SessionFromMessage(_session.Client, channel, id.Id)), channel, id);
			await Domain.ExecutionThread.AddNew(() => NetworkCommon.RunNetwork(_execSession));

			_beginButton.Disable();
			_changeButton.Enable();
			_endButton.Enable();
			return new(Waiting);
		}
		private async Task<NextNetworkInstruction> ChangeOrder(NetworkInstructionArgument arg)
		{
			await _execSession.ChangeOrder();

			//_changeButton.Disable();
			_endButton.Enable();
			return new(Waiting);
		}
		private async Task<NextNetworkInstruction> EndOrderExecution(NetworkInstructionArgument arg)
		{
			await _execSession.StopExecuting();

			_beginButton.Enable();
			_changeButton.Disable();
			_endButton.Disable();
			return new(Waiting);
		}
		private async Task<NextNetworkInstruction> Waiting(NetworkInstructionArgument arg)
		{
			var msg = new UniversalMessageBuilder();
			msg.AddComponents(_beginButton, _changeButton, _endButton);
			msg.AddEmbed(new DiscordEmbedBuilder()
				.WithDescription("**\\*Исполнение заказов\\***"));

			await _session.SendMessage(msg);

			var comp = await _session.GetComponentInteraction();

			if (comp.CompareButton(_beginButton))
				return new(BeginOrderExecution, (comp.Interaction.Channel, comp.Interaction.User));
			if (comp.CompareButton(_changeButton))
				return new(ChangeOrder);
			if (comp.CompareButton(_endButton))
				return new(EndOrderExecution);


			return new();
		}

		public NextNetworkInstruction GetStartingInstruction() => new(Waiting);
		public NextNetworkInstruction GetStartingInstruction(Object payload) => throw new NotImplementedException();
	}
}
