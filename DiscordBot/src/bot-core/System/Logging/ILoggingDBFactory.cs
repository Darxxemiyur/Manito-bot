using Manito.Discord.Database;
using Manito.System.Economy; using Manito.Discord;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.System.Logging
{
	public interface ILoggingDBFactory : IMyDbFactory
	{
		ILoggingDB CreateLoggingDBContext();
		Task<ILoggingDB> CreateLoggingDBContextAsync();
	}
}
