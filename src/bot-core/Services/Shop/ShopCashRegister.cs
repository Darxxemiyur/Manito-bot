using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using Manito.Discord.Db;
using System.Linq;

namespace Manito.Discord.Shop
{

    public class ShopCashRegister
    {
        private IShopDb _myDb;

        public ShopCashRegister(IShopDb myDb)
        {
            _myDb = myDb;
        }
        public IEnumerable<ShopItem> GetShopItems()
        {
            yield return new ShopItem
            {
                Name = "Каркас без насыщения",
                Category = ShopItemCategory.Carcass,
                Price = 10,
            };
            yield return new ShopItem
            {
                Name = "Каркас c насыщением",
                Category = ShopItemCategory.Carcass,
                Price = 50,
            };
            yield return new ShopItem
            {
                Name = "Светляк",
                Category = ShopItemCategory.Plant,
                Price = 100,
            };
            yield return new ShopItem
            {
                Name = "Воскрешение",
                Category = ShopItemCategory.Revive,
                Price = 100000,
            };
            yield return new ShopItem
            {
                Name = "Яйцо",
                Category = ShopItemCategory.Egg,
                Price = 10000,
            };
            yield return new ShopItem
            {
                Name = "Телепорт",
                Category = ShopItemCategory.Teleport,
                Price = 10000,
            };
        }

        public async Task Checkout()
        {

        }
    }

}
