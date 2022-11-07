using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Name.Bayfaderix.Darxxemiyur.Common
{
	/// <summary>
	/// FIFO Fetch Blocking Async Collection | FIFOFBACollection
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class FIFOFBACollection<T> : IDisposable
	{
		public FIFOFBACollection()
		{
			_sync = new();
			_chain = new();
			_prepin = new();
			_chain.Enqueue((_generator = new()).MyTask);
		}

		private MyTaskSource<T> _generator;
		private readonly Queue<Task<T>> _chain;
		private readonly Stack<Task<T>> _prepin;
		private readonly AsyncLocker _sync;

		public Task<bool> HasAny() => Task.FromResult(_chain.Any(x => x.IsCompleted));

		public async Task Handle(T stuff)
		{
			await using var _ = await _sync.BlockAsyncLock();

			if (_generator.MyTask.IsCanceled)
				throw new TaskCanceledException();

			await _generator.TrySetResultAsync(stuff);
			_chain.Enqueue((_generator = new()).MyTask);
		}

		public async Task Cancel()
		{
			await using var _ = await _sync.BlockAsyncLock();
			await _generator.TrySetCanceledAsync();
		}

		public async Task<T> GetData(CancellationToken token = default)
		{
			Task<T> result = null;
			{
				await using var _ = await _sync.BlockAsyncLock();
				result = _prepin.Count > 0 ? _prepin.Pop() : _chain.Dequeue();
			}

			using var revert = new MyTaskSource<T>(token);

			var either = await Task.WhenAny(result, revert.MyTask);

			if (either == revert.MyTask)
			{
				await using var _ = await _sync.BlockAsyncLock();
				_prepin.Push(result);
				await revert.MyTask;
			}

			return await either;
		}

		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					_generator.Dispose();
					_sync.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~FIFOFBACollection() => Dispose(false);
	}

	/// <summary>
	/// First in First out fetch blocking Async Collection FIFOACollection
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class FIFOFBACollection : IDisposable
	{
		public FIFOFBACollection()
		{
			_facade = new();
		}

		private FIFOFBACollection<bool> _facade;

		public Task<bool> HasAny() => _facade.HasAny();

		public Task Handle() => _facade.Handle(true);

		public Task Cancel() => _facade.Cancel();

		public Task GetData(CancellationToken token = default) => _facade.GetData(token);

		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					((IDisposable)_facade).Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~FIFOFBACollection() => Dispose(false);
	}
}