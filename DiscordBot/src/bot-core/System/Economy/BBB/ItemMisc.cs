using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.System.Economy.BBB
{
	public class ItemMisc : ItemBase
	{
		public MiscProperties Properties {
			get; set;
		}
	}
	public enum MiscTypes
	{
		Reskin, EggCheck, GenderSwap
	}
	public class MiscProperties
	{
		public MiscTypes MiscType {
			get; set;
		}
		public int Quantity {
			get; set;
		}
	}
}
