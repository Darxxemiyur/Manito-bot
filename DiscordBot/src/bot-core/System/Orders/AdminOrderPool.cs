using Manito.Discord.Orders;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Manito.Discord.Orders
{
	/// <summary>
	/// Pool of placed orders
	/// </summary>
	public class AdminOrderPool
	{
		private readonly Queue<Order> _queue;
		private readonly Queue<TaskCompletionSource<Order>> _executors;
		private readonly SemaphoreSlim _lock;
		public AdminOrderPool()
		{
			_lock = new(1, 1);
			_queue = new();
			_executors = new();
		}
		private async Task InnerPlaceOrder(Order order)
		{
			if (_executors.Count > 0)
			{
				var rem = _executors.Dequeue();
				if (!rem.Task.IsCanceled)
					rem.TrySetResult(order);
				else
					await InnerPlaceOrder(order);
			}
			else
				_queue.Enqueue(order);
		}
		public async Task PlaceOrder(Order order)
		{
			await _lock.WaitAsync();
			await InnerPlaceOrder(order);
			_lock.Release();
		}
		public async Task<Order> GetOrder(CancellationToken token = default)
		{
			await _lock.WaitAsync(CancellationToken.None);

			if (_queue.Count > 0)
			{
				_lock.Release();
				return _queue.Dequeue();
			}

			var relay = new TaskCompletionSource<Order>();
			_executors.Enqueue(relay);
			token.Register(() => relay.TrySetCanceled());

			_lock.Release();

			return await relay.Task;
		}
	}
}
