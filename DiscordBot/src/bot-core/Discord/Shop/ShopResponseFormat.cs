using DisCatSharp.Entities;

using Manito.Discord.ChatNew;
using Manito.System.Economy;

using System.Collections.Generic;
using System.Linq;

namespace Manito.Discord.Shop
{
	public class ShopResponseFormat
	{
		private ShopCashRegister _cashRegister;
		private PlayerWallet _wallet;

		public ShopResponseFormat(ShopCashRegister cashRegister, PlayerWallet wallet)
		{
			_cashRegister = cashRegister;
			_wallet = wallet;
		}

		public DiscordEmbedBuilder BaseContent(DiscordEmbedBuilder bld = null) => _cashRegister.Default(bld);

		public UniversalMessageBuilder GetDResponse(DiscordEmbedBuilder builder = null) => new UniversalMessageBuilder().AddEmbed(builder ?? BaseContent());

		public UniversalMessageBuilder GetResponse(DiscordEmbedBuilder builder = null) => new(GetDResponse(builder));

		public DiscordEmbedBuilder GetShopItems(DiscordEmbedBuilder prev = null, IEnumerable<ShopItem> list = null) => (list ?? _cashRegister.GetShopItems()).Aggregate(prev ?? BaseContent(), (x, y) => x.AddField(new DiscordEmbedField($"**{y.Name}" + (!y.IsAvailable ? "\n(Недоступно)" : "") + "**", $"**Цена за 1 ед:** {_wallet.CurrencyEmoji} {y.Price}", true)));

		public DiscordSelectComponent GetSelector(IEnumerable<ShopItem> list = null) => new DiscordSelectComponent("Selection", "Выберите товар", (list ?? _cashRegister.GetShopItems()).Where(x => x.IsAvailable).Select(x => new DiscordSelectComponentOption(x.Name, x.Name, $"{x.Price}", false, new DiscordComponentEmoji(_wallet.CurrencyEmojiId))));
	}
}