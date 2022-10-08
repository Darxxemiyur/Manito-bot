using Manito._00_PatternSystems.Common;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Name.Bayfaderix.Darxxemiyur.Common
{
	/// <summary>
	/// Ordered place FIFO, FIFO take, non-blocking async collection.
	/// </summary>
	public class OPFIFOFIFOTCollection<T>
	{
		private readonly Queue<T> _queue;
		private readonly Queue<MyTaskSource<T>> _executors;
		private readonly AsyncLocker _lock;

		public OPFIFOFIFOTCollection()
		{
			_lock = new();
			_queue = new();
			_executors = new();
		}

		private async Task InnerPlaceOrder(T order)
		{
			if (_executors.Count > 0)
			{
				var rem = _executors.Dequeue();
				if (!await rem.TrySetResultAsync(order))
					await InnerPlaceOrder(order);
			}
			else
				_queue.Enqueue(order);
		}

		public async Task PlaceOrder(T order)
		{
			await using var _ = await _lock.BlockAsyncLock();
			await InnerPlaceOrder(order);
		}

		public async Task<bool> AnyOrders()
		{
			await using var _ = await _lock.BlockAsyncLock();
			return _queue.Any();
		}

		private async Task<Task<T>> InnerGetOrder(CancellationToken token = default)
		{
			if (_queue.Count > 0)
			{
				var item = _queue.Dequeue();
				if (token.IsCancellationRequested)
				{
					_queue.Enqueue(item);
					await Task.FromCanceled(token);
				}
				else
					return item == null ? await InnerGetOrder(token) : Task.FromResult(item);
			}

			return Enqueer(token);
		}

		private async Task<T> Enqueer(CancellationToken token)
		{
			var relay = new MyTaskSource<T>(token);
			_executors.Enqueue(relay);

			return await relay.MyTask;
		}

		public async Task<Task<T>> GetOrder(CancellationToken token = default)
		{
			Task<T> orderGet = null;
			await using (var _ = await _lock.BlockAsyncLock())
			{
				orderGet = await InnerGetOrder(token);
			}

			return orderGet;
		}
	}
}