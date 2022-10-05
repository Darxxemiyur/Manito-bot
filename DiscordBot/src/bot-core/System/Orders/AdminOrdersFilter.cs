using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp;
using Manito.Discord.Client;
using Manito.Discord.Shop;
using Manito.Discord;
using Name.Bayfaderix.Darxxemiyur.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Manito.Discord.ChatAbstract;
using Manito.Discord.Chat.DialogueNet;

namespace Manito.Discord.Orders
{
	public class AdminOrdersFilter : IModule
	{
		public Task RunModule() => HandleLoop();
		private async Task HandleLoop()
		{
			while (true)
			{
				var data = (await _queue.GetData()).Item2;
				await HandleAsCommand(data);
			}
		}
		private DialogueNetSessionTab<AdminOrderContext> _aoTab;
		private List<DiscordApplicationCommand> _commandList;
		private DiscordEventProxy<DiscordInteraction> _queue;
		private AdminOrderPool _pool;
		public AdminOrderPool Pool => _pool;
		public AdminOrdersFilter(MyDomain service, EventBuffer eventBuffer)
		{
			_pool = new();
			_aoTab = new(service);
			_commandList = GetCommands().ToList();
			_queue = new();
			service.MyDiscordClient.AppCommands.Add("AdmOrdFlt", _commandList);
			eventBuffer.Interact.OnMessage += FilterMessage;
		}
		private IEnumerable<DiscordApplicationCommand> GetCommands()
		{
			yield return new DiscordApplicationCommand("admin",
			 "Начать администрировать");
#if DEBUG
			yield return new DiscordApplicationCommand("admin_add_test",
			 "Добавить 5 заказов.");
#endif
		}

		private async Task FilterMessage(DiscordClient client, InteractionCreateEventArgs args)
		{
			if (!_commandList.Any(x => args.Interaction.Data.Name.Contains(x.Name)))
				return;

			await _queue.Handle(client, args.Interaction);
			args.Handled = true;
		}
		private async Task HandleAsCommand(DiscordInteraction args)
		{
			if (args.Data.Name.Equals("admin"))
				await _aoTab.CreateSession(new(args), new(), x => Task.FromResult((IDialogueNet)new AdminOrderControl(x, _pool)));
			if (args.Data.Name.Equals("admin_add_test"))
				for (var i = 0; i < 5; i++)
				{
					var order = new Order(0);

					var oid = order.OrderId;

					var id1 = Random.Shared.Next(999);
					var id2 = Random.Shared.Next(999);
					var step1 = new Order.ConfirmationStep(id1, $"Подтверждение игрока {id1}.", $"`/m {id1} Вы подтверждаете исполнение заказа №{oid}?`");

					var step2 = new Order.ConfirmationStep(id2, $"Подтверждение игрока {id2}.", $"`/m {id2} Вы подтверждаете исполнение заказа №{oid}?`");

					var step3 = new Order.CommandStep(id1, $"Телепортирование игрока {id1} к {id2}.", $"`TeleportPToP {id1} {id2}`");

					order.SetSteps(step1, step2, step3);
					await _pool.PlaceOrder(order);
				}

		}
	}
}
