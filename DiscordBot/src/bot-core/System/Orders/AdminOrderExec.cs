using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Manito.Discord.Chat.DialogueNet;
using Manito.Discord.ChatNew;

using Name.Bayfaderix.Darxxemiyur.Node.Network;

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
		public readonly CancellationTokenSource Messanger;
		public readonly UniversalSession Session;
		private readonly AdminOrderPool _pool;
		public AdminOrderExec(AdminOrderPool pool, UniversalSession session)
		{
			_pool = pool;
			Messanger = new();
			Session = session;
		}
		private Order _exOrder;
		public Order ExOrder {
			get => _exOrder;
			set {
				_exOrder = value;
				_steps = value?.Steps?.GetEnumerator();
			}
		}
		private IEnumerator<Order.Step> _steps;
		public async Task ChangeOrder()
		{
			if (ExOrder != null)
				await _pool.PlaceOrder(ExOrder);

			ExOrder = null;
		}
		public async Task StopExecuting()
		{
			await Task.Run(() => Messanger.Cancel());

		}
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

			ExOrder = null;

			return new(FetchNextStep);
		}
		private async Task<NextNetworkInstruction> DoCancellation(NetworkInstructionArgument arg)
		{
			await Session.RemoveMessage();
			await ChangeOrder();

			return new();
		}
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

				await Session.GetComponentInteraction(Messanger.Token);

				asked.Disable();
				success.Enable();
				fail.Enable();
				embed.WithDescription($"{step.Description}\nОпрошено.\nДождитесь ответа игрока.\nВ случае `Нет`, жмите \"{fail.Label}\", в случае `Да`, жмите \"{success.Label}\"");

				await Session.SendMessage(new UniversalMessageBuilder().AddEmbed(embed)
					.AddComponents(asked).AddComponents(fail, success));

				await Session.GetComponentInteraction(Messanger.Token);

				return new(Decider);
			}
			catch (TaskCanceledException)
			{
				return new(DoCancellation);
			}
		}
		private async Task<NextNetworkInstruction> DoCommand(NetworkInstructionArgument arg)
		{
			try
			{
				var step = (Order.ConfirmationStep)arg.Payload;

				var embed = new DiscordEmbedBuilder();
				embed.WithColor(new DiscordColor(255, 255, 0));
				embed.WithDescription($"{step.Description}\nНе опрошено.\nНапишите в чат \"{step.Question}\" и нажмите \"Опрошено.\"");
				var asked = new DiscordButtonComponent(ButtonStyle.Primary, "asked", "Опрошено.");
				var success = new DiscordButtonComponent(ButtonStyle.Success, "success", "Подтвердить.", true);
				var fail = new DiscordButtonComponent(ButtonStyle.Danger, "fail", "Отклонить.", true);
				await Session.SendMessage(new UniversalMessageBuilder().AddEmbed(embed)
					.AddComponents(asked).AddComponents(fail, success));

				await Session.GetComponentInteraction(Messanger.Token);

				asked.Disable();
				success.Enable();
				fail.Enable();
				embed.WithDescription($"{step.Description}\nОпрошено.\nДождитесь ответа игрока.\nВ случае `Нет`, жмите `Отклонить.`, в случае `Да`, жмите `Подтвердить.`");

				await Session.SendMessage(new UniversalMessageBuilder().AddEmbed(embed)
					.AddComponents(asked).AddComponents(fail, success));

				await Session.GetComponentInteraction(Messanger.Token);
				await Session.DoLaterReply();
				return new(Decider);
			}
			catch (TaskCanceledException)
			{
				return new(DoCancellation);
			}
		}
		private async Task<NextNetworkInstruction> FetchNextStep(NetworkInstructionArgument arg)
		{
			try
			{
				var embed = new DiscordEmbedBuilder();
				embed.WithColor(new DiscordColor(255, 255, 0));
				embed.WithDescription($"Ожидание заказов...");
				await Session.SendMessage(new UniversalMessageBuilder().AddEmbed(embed));

				ExOrder = await _pool.GetOrder(Messanger.Token);

				return new(Decider);
			}
			catch (TaskCanceledException)
			{
				return new(DoCancellation);
			}
		}
		public NextNetworkInstruction GetStartingInstruction() => new(FetchNextStep);
		public NextNetworkInstruction GetStartingInstruction(Object payload) => throw new NotImplementedException();
	}
}
