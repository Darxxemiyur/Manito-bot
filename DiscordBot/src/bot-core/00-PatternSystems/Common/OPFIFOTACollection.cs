using Name.Bayfaderix.Darxxemiyur.Common;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manito._00_PatternSystems.Common
{
	/// <summary>
	/// FIFO place, Take all, non-blocking async collection.
	/// </summary>
	public class FIFOPTACollection<T>
	{
		private AsyncLocker _lock;
		private Queue<T> _queue;
		private MyTaskSource _cranck;

		public FIFOPTACollection()
		{
			_lock = new();
			_cranck = new();
			_queue = new();
		}

		public async Task Place(T item)
		{
			await using var _ = await _lock.BlockAsyncLock();
			_queue.Enqueue(item);
			await _cranck.TrySetResultAsync();
		}

		public async Task Place(IEnumerable<T> items)
		{
			await using var _ = await _lock.BlockAsyncLock();
			foreach (var item in items)
				_queue.Enqueue(item);
			await _cranck.TrySetResultAsync();
		}

		public async Task UntilPlaced(CancellationToken token = default)
		{
			var relay = new MyRelayTask(_cranck.MyTask, token);

			await relay.TheTask;
		}

		/// <summary>
		/// Gets all items safely.
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task<IEnumerable<T>> GetAllSafe(CancellationToken token = default)
		{
			await UntilPlaced(token);
			return await GetAll();
		}

		public async Task<IEnumerable<T>> GetAll()
		{
			await using var _ = await _lock.BlockAsyncLock();
			var outQueue = new List<T>();

			while (_queue.Count > 0)
				outQueue.Add(_queue.Dequeue());

			_cranck = new();
			return outQueue;
		}
	}
}