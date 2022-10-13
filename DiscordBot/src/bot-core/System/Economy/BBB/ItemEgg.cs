using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.System.Economy.BBB
{
	public class ItemEgg : ItemBase
	{
		public EggProperties Properties {
			get; set;
		}
	}
	public class EggProperties
	{
		public string Father {
			get; set;
		}

		public string Mother {
			get; set;
		}
	}
}
