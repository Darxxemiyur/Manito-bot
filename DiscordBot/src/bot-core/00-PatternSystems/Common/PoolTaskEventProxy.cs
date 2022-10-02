using Manito.Discord.Orders;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Name.Bayfaderix.Darxxemiyur.Common
{
	/// <summary>
	/// Pool of placed orders
	/// </summary>
	public class PoolTaskEventProxy
	{
		private readonly Queue<Order> _queue;
		private readonly Queue<TaskCompletionSource<Order>> _executors;
		private readonly AsyncLocker _lock;
		public PoolTaskEventProxy()
		{
			_lock = new();
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
			await using var _ = await _lock.BlockAsyncLock();
			await InnerPlaceOrder(order);
		}
		private async Task<Task<Order>> InnerGetOrder(CancellationToken token = default)
		{
			if (_queue.Count > 0)
				return _queue.Dequeue() is var g && !g.OrderCancelledTask.IsCompleted ? Task.FromResult(g) : await InnerGetOrder(token);

			var relay = new TaskCompletionSource<Order>();
			_executors.Enqueue(relay);
			token.Register(() => relay.TrySetCanceled());

			return relay.Task;
		}
		public async Task<Order> GetOrder(CancellationToken token = default)
		{
			Task<Order> orderGet = null;
			await using (var _ = await _lock.BlockAsyncLock(CancellationToken.None))
			{
				orderGet = await InnerGetOrder(token);
			}

			return await orderGet;
		}
	}
}
