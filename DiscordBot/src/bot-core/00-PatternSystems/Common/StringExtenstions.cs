using System;

namespace Manito.Discord.PatternSystems.Common
{
	public static class StringExtenstions
	{
		public static string DoAtMax(this string me, int size) => me[..Math.Min(me.Length, size)];
	}
}