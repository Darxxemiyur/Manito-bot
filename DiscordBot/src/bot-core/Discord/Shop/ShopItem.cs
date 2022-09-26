using System;
using System.Collections;
using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;
using Microsoft.EntityFrameworkCore;

namespace Manito.Discord.Shop
{

    public class ShopItem
    {
        /// <summary>
        /// Name of the Item
        /// </summary>
        public string Name;


        /// <summary>
        /// Category of the Item
        /// </summary>
        public ShopItemCategory Category;
        
        
        /// <summary>
        /// Price for unit of Item
        /// </summary>
        public long Price;
    }

}
