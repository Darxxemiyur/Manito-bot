using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Manito.Discord.Orders
{
	public class Order : IEnumerable<OrderStep>
	{
		private List<OrderStep> _orderSteps;
		public IReadOnlyList<OrderStep> OrderSteps => _orderSteps;
		public Order(params OrderStep[] steps) => _orderSteps = steps.ToList();
		public ulong Initiator;
		public ulong OrderId = OrderIds++;
		private static ulong OrderIds = 0;
		private readonly TaskCompletionSource<string> _handle = new();
		private readonly CancellationTokenSource _cancel = new();
		public Task<string> OrderFinishTask;
		public Task CancelOrder() => Task.Run(_cancel.Cancel);
		public Task MakeUncancellable()
		{
		}
		public Task FinishOrder(string message) => Task.FromResult(_handle.TrySetResult(message));
		public IEnumerator<OrderStep> GetEnumerator() => _orderSteps.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => _orderSteps.GetEnumerator();
	}
}
