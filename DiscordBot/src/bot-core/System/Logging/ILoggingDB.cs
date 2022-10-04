using Manito.Discord.Database;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.System.Logging
{
	public interface ILoggingDB : IMyDatabase
	{
		DbSet<LogLine> LogLines {
			get;
		}
	}
}
