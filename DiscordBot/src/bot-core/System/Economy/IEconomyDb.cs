using Manito.Discord.Database;

using Microsoft.EntityFrameworkCore;

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