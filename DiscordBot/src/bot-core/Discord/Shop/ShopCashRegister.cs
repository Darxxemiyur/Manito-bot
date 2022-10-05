using DisCatSharp.Entities;

using System.Collections.Generic;

namespace Manito.Discord.Shop
{
	public class ShopCashRegister
	{
		private IShopDbFactory _myDb;

		public ShopCashRegister(IShopDbFactory myDb)
		{
			_myDb = myDb;
		}

		public IEnumerable<ShopItem> GetShopItems()
		{
			yield return new ShopItem {
				Name = "Каркас без насыщения",
				Category = ItemCategory.Carcass,
				SpawnCommand = "SpawnCarcass {0} {0} false",
				Price = 1,
			};
			yield return new ShopItem {
				Name = "Каркас c насыщением",
				Category = ItemCategory.SatiationCarcass,
				SpawnCommand = "SpawnCarcass {0} {0} true",
				Price = 4,
			};
			yield return new ShopItem {
				Name = "Светляк",
				Category = ItemCategory.Plant,
				SpawnCommand = "SpawnPlant",
				Price = 40,
			};
			yield return new ShopItem {
				Name = "Воскрешение",
				Category = ItemCategory.Revive,
				Price = 100,
			};
		}

		public DiscordEmbedBuilder Default(DiscordEmbedBuilder bld = null) =>
			(bld ?? new DiscordEmbedBuilder()).WithTitle("~Магазин Манито~")
			.WithColor(DiscordColor.Blurple);
	}
}