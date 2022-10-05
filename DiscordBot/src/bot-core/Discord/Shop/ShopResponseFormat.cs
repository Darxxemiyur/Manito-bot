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

		public DiscordEmbedBuilder BaseContent(DiscordEmbedBuilder bld = null) =>
			_cashRegister.Default(bld);

		public UniversalMessageBuilder GetDResponse(DiscordEmbedBuilder builder = null)
		{
			return new UniversalMessageBuilder().AddEmbed(builder ?? BaseContent());
		}

		public UniversalMessageBuilder GetResponse(DiscordEmbedBuilder builder = null)
		{
			return new(GetDResponse(builder));
		}

		public DiscordEmbedBuilder GetShopItems(DiscordEmbedBuilder prev = null,
		 IEnumerable<ShopItem> list = null)
		{
			var emb = prev ?? BaseContent();
			var str = (list ?? _cashRegister.GetShopItems()).Aggregate(emb, (x, y) => {
				var price = $"{_wallet.CurrencyEmoji} {y.Price}";
				return x.AddField($"**{y.Name}**", $"**Цена за 1 ед:** {price}", true);
			});
			return emb;
		}

		public DiscordSelectComponent GetSelector(IEnumerable<ShopItem> list = null)
		{
			var items = (list ?? _cashRegister.GetShopItems())
			.Select(x => new DiscordSelectComponentOption(x.Name, x.Name, $"{x.Price}",
				false, new DiscordComponentEmoji(_wallet.CurrencyEmojiId)));
			return new DiscordSelectComponent("Selection", "Выберите товар", items);
		}
	}
}