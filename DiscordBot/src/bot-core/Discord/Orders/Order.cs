using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.Discord.Orders
{
	public class Order
	{
		private List<OrderStep> _orderSteps;
		public IReadOnlyList<OrderStep> OrderSteps => _orderSteps;
		public Order(params OrderStep[] steps) => _orderSteps = steps.ToList();
		public ulong Initiator;
		public ulong OrderId = OrderIds++;
		private static ulong OrderIds = 0;
	}
}
