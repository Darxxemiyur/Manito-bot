using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.System.Economy.BBB
{
	public abstract class ItemBase
	{
		public long Id {
			get; set;
		}
		public long Owner {
			get; set;
		}
	}
}
