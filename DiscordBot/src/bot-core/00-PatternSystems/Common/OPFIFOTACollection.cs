using Name.Bayfaderix.Darxxemiyur.Common;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manito._00_PatternSystems.Common
{
	/// <summary>
	/// Ordered place FIFO, Take all, non-blocking async collection.
	/// </summary>
	public class OPFIFOTACollection<T>
	{
		private AsyncLocker _lock;
		private Queue<T> _queue;
		private TaskCompletionSource _cranck;

		public OPFIFOTACollection()
		{
			_lock = new();
			_cranck = new();
			_queue = new();
		}

		public async Task Place(T item)
		{
			await using var _ = await _lock.BlockAsyncLock();
			_queue.Enqueue(item);
			_cranck.TrySetResult();
		}

		public async Task Place(IEnumerable<T> items)
		{
			await using var _ = await _lock.BlockAsyncLock();
			foreach (var item in items)
				_queue.Enqueue(item);
			_cranck.TrySetResult();
		}

		public async Task UntilPlaced(CancellationToken token = default)
		{
			var canc = new CancellationTokenSource();
			var allcanc = CancellationTokenSource.CreateLinkedTokenSource(token, canc.Token).Token;
			var timeout = Task.Delay(-1, allcanc);

			var comp = await Task.WhenAny(_cranck.Task, timeout);

			if (comp != timeout)
				canc.Cancel();

			await comp;
		}

		public async Task<IEnumerable<T>> GetAll()
		{
			await using var _ = await _lock.BlockAsyncLock();
			var outQueue = new List<T>();

			while (_queue.Count > 0)
				outQueue.Add(_queue.Dequeue());

			return outQueue;
		}
	}
}