using Manito.Discord.Database;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.Discord.Rules
{
	public interface IRulesDb : IMyDatabase
	{
		DbSet<RulesPoint> Rules {
			get;
		}
	}
}
