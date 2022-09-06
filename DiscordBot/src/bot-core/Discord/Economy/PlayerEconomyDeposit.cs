using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;


namespace Manito.Discord.Economy
{

    public class PlayerEconomyDeposit
    {

        public ulong DiscordID;
        public long Currency;
    }
}
