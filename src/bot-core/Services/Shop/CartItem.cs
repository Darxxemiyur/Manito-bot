using System;
using System.Collections;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace Manito.Discord.Shop
{

    public class CartItem
    {
        /// <summary>
        /// Name of the Item
        /// </summary>
        public String Name => ShopItem.Name;


        /// <summary>
        /// Item of the shop
        /// </summary>
        public ShopItem ShopItem;


        /// <summary>
        /// Price for unit of Item
        /// </summary>
        public long Price => ShopItem.Price * Amount;
        public long Amount;

        public CartItem(ShopItem item, uint amount)
        {
            ShopItem = item;
            Amount = amount;
        }
    }

}
