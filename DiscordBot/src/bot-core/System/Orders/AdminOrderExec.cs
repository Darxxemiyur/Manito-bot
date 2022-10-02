using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Manito.Discord.Chat.DialogueNet;
using Manito.Discord.ChatNew;

using Name.Bayfaderix.Darxxemiyur.Node.Network;
using Name.Bayfaderix.Darxxemiyur.Common;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manito.Discord.Orders
{
	public class AdminOrderExec : IDialogueNet
	{
		public NodeResultHandler StepResultHandler {
			get;
		} = Common.DefaultNodeResultHandler;
		public readonly UniversalSession Session;
		private CancellationTokenSource _quitToken;
		private CancellationTokenSource _swapToken;
		private CancellationTokenSource _cancelOrder;
		private CancellationTokenSource _localToken;
		private readonly PoolTaskEventProxy _pool;
		private readonly DiscordChannel _channel;
		private readonly DiscordUser _admin;
		private bool _swap;
		private IEnumerator<Order.Step> _steps;
		private Order _exOrder;
		public Order ExOrder {
			get => _exOrder;
			set {
				_exOrder = value;
				_steps = value?.Steps?.GetEnumerator();
			}
		}
		public AdminOrderExec(PoolTaskEventProxy pool, UniversalSession session, DiscordChannel channel, DiscordUser user)
		{
			_pool = pool;
			_quitToken = new();
			_swapToken = new();
			_cancelOrder = new();
			Session = session;
			_channel = channel;
			_admin = user;
		}
		public Task StopExecuting() => Task.Run(() => _quitToken.Cancel());
		private async Task<NextNetworkInstruction> Decider(NetworkInstructionArgument arg)
		{
			_steps.MoveNext();
			var step = _steps.Current;
			if (step != null)
			{
				if (step.Type == Order.StepType.Confirmation)
					return new(DoConfirmation, step);
				if (step.Type == Order.StepType.Command)
					return new(DoCommand, step);

				throw new NotImplementedException();
			}

			await ExOrder.FinishOrder();
			ExOrder = null;

			return new(FetchNextStep);
		}
		private async Task<NextNetworkInstruction> DoOrderCancellation(NetworkInstructionArgument arg)
		{
			if (_swapToken.IsCancellationRequested)
			{
				await _pool.PlaceOrder(ExOrder);
				ExOrder = null;
				_swapToken = new();
				return new(FetchNextStep);
			}

			if (_quitToken.IsCancellationRequested)
			{
				_quitToken = new();
				await _pool.PlaceOrder(ExOrder);
				ExOrder = null;
				await Session.RemoveMessage();
				return new();
			}

			if (_cancelOrder.Token.IsCancellationRequested)
			{
				_cancelOrder = new();
				await ExOrder.CancelOrder();
			}

			ExOrder = null;
			return new(FetchNextStep);
		}
		public Task ChangeOrder() => Task.Run(_swapToken.Cancel);

		private async Task<NextNetworkInstruction> DoConfirmation(NetworkInstructionArgument arg)
		{
			try
			{
				var step = (Order.ConfirmationStep)arg.Payload;

				var asked = new DiscordButtonComponent(ButtonStyle.Primary, "asked", "Опрошено");
				var success = new DiscordButtonComponent(ButtonStyle.Success, "success", "Подтвердить", true);
				var fail = new DiscordButtonComponent(ButtonStyle.Danger, "fail", "Отклонить", true);
				var embed = new DiscordEmbedBuilder();
				embed.WithColor(new DiscordColor(255, 255, 0));
				embed.WithDescription($"{step.Description}\nНе опрошено.\nНапишите в чат \"{step.Question}\" и нажмите \"{asked.Label}\"");
				await Session.SendMessage(new UniversalMessageBuilder().AddEmbed(embed)
					.AddComponents(asked).AddComponents(fail, success));

				await Session.GetComponentInteraction(_localToken.Token);

				asked.Disable();
				success.Enable();
				fail.Enable();
				embed.WithDescription($"{step.Description}\nОпрошено.\nДождитесь ответа игрока.\nВ случае `Нет`, жмите \"{fail.Label}\", в случае `Да`, жмите \"{success.Label}\"");

				await Session.SendMessage(new UniversalMessageBuilder().AddEmbed(embed)
					.AddComponents(asked).AddComponents(fail, success));

				var answer = await Session.GetComponentInteraction(_localToken.Token);

				if (answer.CompareButton(success))
					return new(Decider);
				return new();
			}
			catch (TaskCanceledException)
			{
				return new(DoOrderCancellation);
			}
		}
		private async Task<NextNetworkInstruction> DoCommand(NetworkInstructionArgument arg)
		{
			try
			{
				var step = (Order.CommandStep)arg.Payload;

				var asked = new DiscordButtonComponent(ButtonStyle.Primary, "executed", "Выполнено.");
				var embed = new DiscordEmbedBuilder();
				embed.WithColor(new DiscordColor(255, 255, 0));
				embed.WithDescription($"{step.Description}\nНапишите в консоль \"{step.Command}\" и нажмите \"{asked.Label}\"");
				await Session.SendMessage(new UniversalMessageBuilder().AddEmbed(embed)
					.AddComponents(asked));

				await Session.GetComponentInteraction(_localToken.Token);
				await Session.DoLaterReply();

				return new(Decider);
			}
			catch (TaskCanceledException)
			{
				return new(DoOrderCancellation);
			}
		}
		private async Task<NextNetworkInstruction> FetchNextStep(NetworkInstructionArgument arg)
		{
			try
			{
				_localToken = null;

				var embed = new DiscordEmbedBuilder();
				embed.WithColor(new DiscordColor(255, 255, 0));
				embed.WithDescription($"Ожидание заказов...");
				await Session.SendMessage(new UniversalMessageBuilder().AddEmbed(embed));

				ExOrder = await _pool.GetOrder(_quitToken.Token);

				_localToken = CancellationTokenSource.CreateLinkedTokenSource(_swapToken.Token, _quitToken.Token, ExOrder.PlayerOrderCancelToken, _cancelOrder.Token);

				var msg = await _channel.SendMessageAsync(new UniversalMessageBuilder()
					.SetContent($"<@{_admin.Id}>").AddMention(new UserMention(_admin)));

				await Session.Client.Domain.ExecutionThread.AddNew(() => msg.DeleteAsync());

				return new(Decider);
			}
			catch (TaskCanceledException)
			{
				return new(DoOrderCancellation);
			}
		}
		public NextNetworkInstruction GetStartingInstruction() => new(FetchNextStep);
		public NextNetworkInstruction GetStartingInstruction(Object payload) => throw new NotImplementedException();
	}
}
