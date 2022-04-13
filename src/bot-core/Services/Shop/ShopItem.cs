using System;
using System.Collections;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace Manito.Discord.Shop
{

    public class ShopItem
    {
        /// <summary>
        /// Name of the Item
        /// </summary>
        public String Name;


        /// <summary>
        /// Category of the Item
        /// </summary>
        public ShopItemCategory Category;
        
        
        /// <summary>
        /// Price for unit of Item
        /// </summary>
        public UInt64 Price;
    }

}
