using System;
using Microsoft.EntityFrameworkCore;

using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;
using Manito.Discord.Database;

namespace Manito.Discord.Economy
{

	public interface IEconomyDb : IMyDatabase
	{
		DbSet<PlayerEconomyDeposit> PlayerEconomies {
			get;
		}
	}

}
