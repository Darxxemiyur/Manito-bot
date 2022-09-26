using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;

using Manito.Discord.Database;
using System.Linq;

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
            yield return new ShopItem
            {
                Name = "Каркас без насыщения",
                Category = ShopItemCategory.Carcass,
                Price = 1,
            };
            yield return new ShopItem
            {
                Name = "Каркас c насыщением",
                Category = ShopItemCategory.SatiationCarcass,
                Price = 4,
            };
            yield return new ShopItem
            {
                Name = "Светляк",
                Category = ShopItemCategory.Plant,
                Price = 40,
            };
            yield return new ShopItem
            {
                Name = "Воскрешение",
                Category = ShopItemCategory.Revive,
                Price = 100,
            };
        }
        public DiscordEmbedBuilder Default(DiscordEmbedBuilder bld = null) =>
            (bld ?? new DiscordEmbedBuilder()).WithTitle("~Магазин Манито~")
            .WithColor(DiscordColor.Blurple);
    }

}
