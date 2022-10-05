using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Manito.Discord.Chat.DialogueNet;
using Manito.Discord.ChatNew;

using Name.Bayfaderix.Darxxemiyur.Node.Network;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Manito.Discord.Shop
{
	public class ShopDialogue : IDialogueNet
	{
		private DialogueTabSession<ShopContext> _session;

		public ShopDialogue(DialogueTabSession<ShopContext> session) => _session = session;

		public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;

		private Task StopSession() => _session.EndSession();

		private IDialogueNet DialogNetwork(ShopItem item) => item.Category switch {
			ItemCategory.SatiationCarcass or ItemCategory.Carcass =>
				new BuyingStepsForMeatFood(_session, item),
			ItemCategory.Plant => new BuyingStepsForPlantFood(_session, item),
			_ => new BuyingStepsForError(_session),
		};

		public async Task<NextNetworkInstruction> EnterMenu(NetworkInstructionArgument arg)
		{
			try
			{
				var exbtn = new DiscordButtonComponent(ButtonStyle.Danger, "Exit", "Выйти");
				while (true)
				{
					var shopItems = _session.Context.CashRegister.GetShopItems();
					var items = _session.Context.Format.GetSelector(shopItems);
					var mg = _session.Context.Format.GetResponse(_session.Context.Format.GetShopItems(null, shopItems)).AddComponents(items).AddComponents(exbtn);
					await _session.SendMessage(mg);

					var argv = await _session.GetComponentInteraction();

					if (argv.CompareButton(exbtn))
						break;

					await _session.DoLaterReply();
					await ItemSelected(argv.GetOption(shopItems.ToDictionary(x => x.Name)));
				}

				await _session.SendMessage(_session.Context.Format.GetResponse(_session.Context.Format.BaseContent().WithDescription("Сессия успешно завершена.")));
				await Task.Delay(5000);
				await _session.RemoveMessage();

				await StopSession();
			}
			catch (TimeoutException)
			{
				var ms = "Сессия завершена по причине привышения времени ожидания взаимодействия.";
				await _session.SendMessage(_session.Context.Format.GetDResponse(_session.Context.Format.BaseContent().WithDescription(ms)));
				await Task.Delay(5000);
				await _session.RemoveMessage();
				await StopSession();
			}

			return new();
		}

		private async Task ItemSelected(ShopItem item)
		{
			var chain = DialogNetwork(item);
			await NetworkCommon.RunNetwork(chain);
		}

		public NextNetworkInstruction GetStartingInstruction() => new(EnterMenu);

		public NextNetworkInstruction GetStartingInstruction(Object payload) => new(EnterMenu);
	}
}