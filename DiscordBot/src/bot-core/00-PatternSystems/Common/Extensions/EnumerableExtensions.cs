using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Manito.Discord.Inventory;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Name.Bayfaderix.Darxxemiyur.Common
{
	public static class EnumerableExtensions
	{
		public static int GetSequenceHashCode<TItem>(this IEnumerable<TItem> list)
		{
			if (list == null)
				return 0;
			const int seedValue = 0x2D2816FE;
			const int primeNumber = 397;
			return list.Aggregate(seedValue + list.GetHashCode(), (current, item) => (current * primeNumber) + (Equals(item, default(TItem)) ? 0 : item.GetHashCode()));
		}
		public static IEnumerable<TItem> AsSaturatedTape<TItem>(this IEnumerable<TItem> components, Func<int, TItem> onLeft, Func<int, TItem> onRight, int maxPerChunk, Func<int, TItem> filler)
		{
			components = AsMarkedTape(components, onLeft, onRight, maxPerChunk);
			var max = components.Count();
			var chunks = (int)Math.Ceiling((double)max / maxPerChunk) * maxPerChunk - max;
			return components.Concat(Enumerable.Range(1, chunks).Select(x => filler(x)));
		}
		public static IEnumerable<TItem> AsMarkedTape<TItem>(this IEnumerable<TItem> components, Func<int, TItem> onLeft, Func<int, TItem> onRight, int maxPerChunk)
		{
			var max = components.Count();
			var i = 0;
			foreach (var component in components)
			{
				if (i % maxPerChunk == 0 && i > 0)
				{
					i++;
					max++;
					yield return onLeft(i);
				}
				i++;
				yield return component;
				if (i % maxPerChunk == maxPerChunk - 1 && i < max - 1)
				{
					i++;
					max++;
					yield return onRight(i);
				}
			}
		}
	}
}
