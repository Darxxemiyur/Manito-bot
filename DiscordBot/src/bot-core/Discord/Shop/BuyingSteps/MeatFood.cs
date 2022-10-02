using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DisCatSharp.Enums;
using System.Reflection;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;
using Microsoft.EntityFrameworkCore;

using Manito.Discord.Chat.DialogueNet;
using Name.Bayfaderix.Darxxemiyur.Node.Network;
using Manito.Discord.ChatNew;
using Manito.Discord.Orders;
using System.Diagnostics;
using System.Threading;

namespace Manito.Discord.Shop
{
	public class BuyingStepsForMeatFood : IDialogueNet
	{
		private ShopItem _food;
		private DialogueTabSession<ShopContext> _session;
		private int _quantity;
		public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;
		public NextNetworkInstruction GetStartingInstruction(object payload) => GetStartingInstruction();

		public NextNetworkInstruction GetStartingInstruction() => new(SelectQuantity, NextNetworkActions.Continue);
		public BuyingStepsForMeatFood(DialogueTabSession<ShopContext> session, ShopItem food)
		{
			_session = session;
			_food = food;
		}

		private async Task<NextNetworkInstruction> SelectQuantity(NetworkInstructionArgument args)
		{
			var btns = Common.Generate;
			var ms1 = $"Выберите количество {_food.Name}";
			var price = _food.Price;

			var qua = await Common.GetQuantity(new[] { -5, -2, 1, 2, 5 }, new[] { 1, 10, 100 },
				_session,
			 async (x, y) => y < 0 || await _session.Context.Wallet.CanAfford((x + y) * price),
			 async x => _session.Context.Format.GetResponse(_session.Context.Format.BaseContent()
			 .WithDescription($"{ms1}\nВыбранное количество {x} кг за {x * price}.")), _quantity);

			if (!qua.HasValue)
				return new NextNetworkInstruction(null, NextNetworkActions.Stop);

			if ((_quantity = qua.Value) <= 0)
				return new(SelectQuantity, NextNetworkActions.Continue);

			return new(ExecuteTransaction, NextNetworkActions.Continue);
		}
		private async Task<NextNetworkInstruction> ExecuteTransaction(NetworkInstructionArgument args)
		{
			var wallet = _session.Context.Wallet;
			var resp = _session.Context.Format;

			var price = _quantity * _food.Price;

			if (!await wallet.CanAfford(price))
				return new NextNetworkInstruction(ForceChange, NextNetworkActions.Continue);

			await wallet.Withdraw(price, $"Покупка {_food.Name} за {_food.Price} в кол-ве {_quantity} за {price}");


			return new NextNetworkInstruction(GetId);
		}
		private async Task<NextNetworkInstruction> GetId(NetworkInstructionArgument args)
		{
			var id = 0;
			while (true)
			{
				var qua = await Common.GetQuantity(new[] { -5, -2, 1, 2, 5 }, new[] { 1, 10, 100 },
				_session, (x, y) => Task.FromResult(true),
				async x => _session.Context.Format.GetResponse(_session.Context.Format.BaseContent()
				.WithDescription($"ID получающий ваш заказ - {x}")), id);

				id = qua ?? id;

				var cont = new DiscordButtonComponent(ButtonStyle.Primary, "continue", "Продолжить");
				var back = new DiscordButtonComponent(ButtonStyle.Danger, "back", "Изменить");

				await _session.SendMessage(new UniversalMessageBuilder().SetContent($"ID: {id}\nПродолжить?").AddComponents(back, cont));
				var intr = await _session.GetComponentInteraction();

				if (intr.CompareButton(cont))
					break;

			}
			return new(WaitForOrder, id);
		}
		private async Task<NextNetworkInstruction> WaitForOrder(NetworkInstructionArgument args)
		{
			var id = (uint)(int)args.Payload;
			var rmsg = new DiscordEmbedBuilder();
			rmsg.WithDescription("Ожидание исполнения Вашего заказа.");

			var cancelBtn = new DiscordButtonComponent(ButtonStyle.Primary, "cancel", "Отменить");

			var order = new Order(_session.Context.CustomerId);
			var seq = new List<Order.Step>();
			seq.Add(new Order.ConfirmationStep(id, $"Подтвердите получение каркаса на {_quantity} игроку {id}", $"`/m {id} Вы подтверждаете получение каркаса на {_quantity}? (Да/Нет)`"));
			seq.Add(new Order.ChangeStateStep());
			seq.Add(new Order.CommandStep(id, $"Телепортирование к {id}", $"`TeleportToP {id}`"));
			var size = _quantity;
			while (size > 0)
			{
				var single = Math.Min(size, 2000);
				size -= single;
				seq.Add(new Order.CommandStep(id, $"Выдача каркаса на {single} игроку {id}", $"`SpawnCarcass {single} {single}`"));
			}

			order.SetSteps(seq);

			await _session.Client.Domain.Filters.AdminOrder.Pool.PlaceOrder(order);

			var not1 = order.OrderCompleteTask;
			var noRet = new CancellationTokenSource();
			var not2 = Task.Run(async () => {
				await order.OrderNonCancellableTask;
				await Task.Run(noRet.Cancel);
			});

			var not3 = order.OrderCancelledTask;

			await _session.SendMessage(new UniversalMessageBuilder().AddEmbed(rmsg).AddComponents(cancelBtn));
			var both = CancellationTokenSource.CreateLinkedTokenSource(order.AdminOrderCancelToken, noRet.Token);
			var res = _session.GetComponentInteraction(both.Token);
			var list = new List<Task> { not1, not2, not3, res };

			while (true)
			{
				var first = await Task.WhenAny(list);
				list.Remove(first);

				if (first == not2 || first == res)
				{
					await _session.SendMessage(new UniversalMessageBuilder().AddEmbed(rmsg).AddComponents(cancelBtn.Disable()));
				}
				if (first == res && !first.IsCanceled)
				{
					await order.TryCancelOrder();
					await not3;
					break;
				}
				if (first == not2)
				{
					await not1;
					break;
				}
				if (first == not3)
				{
					break;
				}
			}

			return new();
		}
		private async Task<NextNetworkInstruction> ForceChange(NetworkInstructionArgument args)
		{
			var wallet = _session.Context.Wallet;
			var resp = _session.Context.Format;

			var price = _quantity * _food.Price;
			var ms1 = $"Вы не можете позволить {_quantity} {_food.Name} за {price}.";
			var ms2 = $"Пожалуйста измените выбранное количество {_food.Name} и попробуйте снова.";
			var rsp = _session.Context.Format.GetResponse(_session.Context.Format.BaseContent().WithDescription($"{ms1}\n{ms2}"));

			var cancel = new DiscordButtonComponent(ButtonStyle.Danger, "Cancel", "Отмена");
			var chnamt = new DiscordButtonComponent(ButtonStyle.Primary, "Back", "Изменить кол-во");
			rsp.AddComponents(cancel, chnamt);

			await _session.SendMessage(rsp);

			var argv = await _session.GetComponentInteraction();

			if (argv.CompareButton(chnamt))
				return new(SelectQuantity, NextNetworkActions.Continue);

			return new(null);
		}
	}
}