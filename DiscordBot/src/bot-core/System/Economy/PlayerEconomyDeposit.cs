using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;


namespace Manito.Discord.Economy
{

    public class PlayerEconomyDeposit
    {

        public ulong DiscordID;
        public long Currency;
    }
}
