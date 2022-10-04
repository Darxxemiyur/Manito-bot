using System;
using Microsoft.EntityFrameworkCore;

using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;
using Manito.Discord.Database;
using Manito.System.Economy; using Manito.Discord;

namespace Manito.System.Economy
{
	public interface IEconomyDb : IMyDatabase
	{
		DbSet<PlayerEconomyDeposit> PlayerEconomies {
			get;
		}
		DbSet<PlayerEconomyWork> PlayerWorks {
			get;
		}
	}

}
