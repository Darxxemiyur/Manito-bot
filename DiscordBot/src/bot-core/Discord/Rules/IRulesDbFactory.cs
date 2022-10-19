using Manito.Discord.Database;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.Discord.Rules
{
	public interface IRulesDbFactory : IMyDbFactory
	{
		IRulesDb CreateMyDbContext();

		Task<IRulesDb> CreateMyDbContextAsync();
	}
}
