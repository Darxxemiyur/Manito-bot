using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.Discord.PatternSystems.Common
{
	public static class StringExtenstions
	{
		public static string DoAtMax(this string me, int size) => me[..(Math.Min(me.Length, size) - 1)];
	}
}
