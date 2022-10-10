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

		private async Task InnerPlaceItem(T order)
		{
			if (_executors.Count > 0)
			{
				var rem = _executors.Dequeue();
				if (!await rem.TrySetResultAsync(order))
					await InnerPlaceItem(order);
			}
			else
				_queue.Enqueue(order);
		}

		public async Task PlaceItem(T order)
		{
			await using var _ = await _lock.BlockAsyncLock();
			await InnerPlaceItem(order);
		}

		public async Task<bool> AnyItems()
		{
			await using var _ = await _lock.BlockAsyncLock();
			return _queue.Any();
		}

		private async Task<Task<T>> InnerGetItem(CancellationToken token = default)
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
					return item == null ? await InnerGetItem(token) : Task.FromResult(item);
			}

			return Enquer(token);
		}

		private async Task<T> Enquer(CancellationToken token)
		{
			var relay = new MyTaskSource<T>(token);
			_executors.Enqueue(relay);

			return await relay.MyTask;
		}

		public async Task<Task<T>> GetItem(CancellationToken token = default)
		{
			Task<T> orderGet = null;
			await using (var _ = await _lock.BlockAsyncLock())
			{
				orderGet = await InnerGetItem(token);
			}

			return orderGet;
		}
	}
}