using System;
using System.Collections.Generic;

namespace Manito.Discord.PatternSystems.Common
{
	public static class StringExtenstions
	{
		public static string DoStartAtMax
			(this string me, int size) => me[..Math.Min(me.Length, size)];
		public static string DoEndAtMax
			(this string me, int size) => me.NotDoEndAtMax(me.Length - size);
		public static string NotDoStartAtMax
			(this string me, int size) => me.DoStartAtMax(me.Length - size);
		public static string NotDoEndAtMax
			(this string me, int size) => me[Math.Min(me.Length, size)..me.Length];
	}
}