using DSharpPlus.Entities;

using Manito.Discord.Economy;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public DiscordMessageBuilder GetDResponse(DiscordEmbedBuilder builder = null)
		{
			return new DiscordMessageBuilder().WithEmbed(builder ?? BaseContent());
		}
		public DiscordInteractionResponseBuilder GetResponse(DiscordEmbedBuilder builder = null)
		{
			return new DiscordInteractionResponseBuilder(GetDResponse(builder));
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
