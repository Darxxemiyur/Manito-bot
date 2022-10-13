using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.System.Economy.BBB
{
	public interface IBBBDb
	{
		DbSet<ItemBase> InventoryItems {
			get;
		}
	}
}
