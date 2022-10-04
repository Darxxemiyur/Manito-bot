using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;


namespace Manito.System.Economy
{

	public class PlayerEconomyDeposit
	{
		public ulong DiscordID {
			get; set;
		}
		public long ScalesCurr {
			get; set;
		}
		public long ChupatCurr {
			get; set;
		}
		public long DonatCurr {
			get; set;
		}
		public PlayerEconomyDeposit(ulong discordID)
		{
			DiscordID = discordID;
			ScalesCurr = 5000;
			ChupatCurr = 0;
			DonatCurr = 0;
		}
	}
}
