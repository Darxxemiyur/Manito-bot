using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Enums;
using Microsoft.EntityFrameworkCore;

using Manito.Discord.Chat.DialogueNet;
using Name.Bayfaderix.Darxxemiyur.Node.Network;
using Manito.Discord.ChatNew;
using Manito.Discord.Economy;

namespace Manito.Discord.Shop
{
	public class BuyingStepsForPlantFood : IDialogueNet
	{
		private ShopItem _food;
		private DialogueTabSession<ShopContext> _session;
		private PlayerWallet Wallet => _session.Context.Wallet;
		private int _quantity;

		public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;
		public NextNetworkInstruction GetStartingInstruction(object payload) => GetStartingInstruction();
		public NextNetworkInstruction GetStartingInstruction() => new(SelectQuantity, NextNetworkActions.Continue);
		public BuyingStepsForPlantFood(DialogueTabSession<ShopContext> session, ShopItem food)
		{
			_session = session;
			_food = food;
		}

		private async Task<NextNetworkInstruction> SelectQuantity(NetworkInstructionArgument args)
		{
			var ms1 = $"Выберите количество {_food.Name}";
			var price = _food.Price;
			var wallet = _session.Context.Wallet;
			var resp = _session.Context.Format;

			var qua = await Common.GetQuantity(new[] { -5, -2, 1, 2, 5 }, new[] { 1, 5, 10 }, _session, async (x, y) => y < 0 || await wallet.CanAfford((x + y) * price),
			 async x => resp.GetResponse(resp.BaseContent()
			 .WithDescription($"{ms1}\nВыбранное количество {x} шт за {x * price}.")), _quantity);

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
			//var inventory = _session.Context.Inventory;
			var price = _quantity * _food.Price;

			if (!await wallet.CanAfford(price))
				return new NextNetworkInstruction(ForceChange, NextNetworkActions.Continue);

			await wallet.Withdraw(price, $"Покупка {_food.Name} за {_food.Price} в кол-ве {_quantity} за {price}");
			//await inventory.AddItem(x => (x.ItemType, x.Owner, x.Quantity)
			// = ($"{_food.Category}", _session.Customer.Id, _quantity));

			return new NextNetworkInstruction(null, NextNetworkActions.Stop);
		}
		private async Task<NextNetworkInstruction> ForceChange(NetworkInstructionArgument args)
		{
			var wallet = _session.Context.Wallet;
			var resp = _session.Context.Format;
			//var inventory = _session.Context.Inventory;

			var price = _quantity * _food.Price;
			var ms1 = $"Вы не можете позволить {_quantity} {_food.Name} за {price}.";
			var ms2 = $"Пожалуйста измените выбранное количество {_food.Name} и попробуйте снова.";
			var rsp = resp.GetResponse(resp.BaseContent().WithDescription($"{ms1}\n{ms2}"));

			var cancel = new DiscordButtonComponent(ButtonStyle.Danger, "Cancel", "Отмена");
			var chnamt = new DiscordButtonComponent(ButtonStyle.Primary, "Back", "Изменить кол-во");
			rsp.AddComponents(cancel, chnamt);

			await _session.SendMessage(rsp);

			var argv = await _session.GetComponentInteraction();

			if (argv.CompareButton(chnamt))
				return new(SelectQuantity, NextNetworkActions.Continue);

			return new();
		}
	}
}