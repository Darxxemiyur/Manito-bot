using System;
using System.Threading;
using System.Threading.Tasks;

namespace Name.Bayfaderix.Darxxemiyur.Common
{
	public class MyTaskSource : IDisposable
	{
		private readonly MyTaskSource<bool> _facade;
		private bool disposedValue;

		public MyTaskSource(CancellationToken token = default) => _facade = new(token);

		public Task MyTask => _facade.MyTask;

		public Task<bool> TrySetResultAsync() => _facade.TrySetResultAsync(false);

		public Task<bool> TrySetCanceledAsync() => _facade.TrySetCanceledAsync();

		public Task<bool> TrySetExceptionAsync(Exception exception) => _facade.TrySetExceptionAsync(exception);

		public bool TrySetResult() => _facade.TrySetResult(false);

		public bool TrySetCanceled() => _facade.TrySetCanceled();

		public bool TrySetException(Exception exception) => _facade.TrySetException(exception);

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					_facade.Dispose();
				}

				disposedValue = true;
			}
		}

		~MyTaskSource() => Dispose(disposing: false);

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}

	public class MyTaskSource<T> : IDisposable
	{
		private readonly TaskCompletionSource<T> _source;
		private readonly AsyncLocker _lock;
		private readonly CancellationTokenSource _cancel;
		private readonly CancellationToken _inner;

		public MyTaskSource(CancellationToken token = default)
		{
			_lock = new();
			_source = new();
			_cancel = new CancellationTokenSource();
			_inner = CancellationTokenSource.CreateLinkedTokenSource(token, _cancel.Token).Token;
		}

		private Task<T> _innerTask;
		private bool disposedValue;

		public Task<T> MyTask => InSecure();

		public static implicit operator Task<T>(MyTaskSource<T> task) => task.MyTask;

		private async Task<T> InSecure()
		{
			{
				await using var _ = await _lock.BlockAsyncLock();
				_innerTask ??= InTask();
			}
			return await _innerTask;
		}

		private async Task<T> InTask()
		{
			using var vDancell = new CancellationTokenSource();
			using var vCancell = CancellationTokenSource.CreateLinkedTokenSource(vDancell.Token, _inner);

			var timeout = Task.Delay(-1, vCancell.Token);
			var theTask = await Task.WhenAny(_source.Task, timeout);
			if (theTask == timeout)
				_source.SetCanceled(vCancell.Token);
			if (theTask == _source.Task)
				vDancell.Cancel();

			return await _source.Task;
		}

		public bool TrySetResult(T result)
		{
			using var _ = _lock.BlockLock();

			return !_inner.IsCancellationRequested && _source.TrySetResult(result);
		}

		public bool TrySetException(Exception result)
		{
			using var _ = _lock.BlockLock();

			return !_inner.IsCancellationRequested && _source.TrySetException(result);
		}

		public bool TrySetCanceled()
		{
			using var _ = _lock.BlockLock();

			if (!_inner.IsCancellationRequested)
				_cancel.Cancel();

			return _inner.IsCancellationRequested;
		}

		public async Task<bool> TrySetResultAsync(T result)
		{
			await using var _ = await _lock.BlockAsyncLock();

			return !_inner.IsCancellationRequested && await Task.Run(() => _source.TrySetResult(result));
		}

		public async Task<bool> TrySetExceptionAsync(Exception result)
		{
			await using var _ = await _lock.BlockAsyncLock();

			return !_inner.IsCancellationRequested && await Task.Run(() => _source.TrySetException(result));
		}

		public async Task<bool> TrySetCanceledAsync()
		{
			await using var _ = await _lock.BlockAsyncLock();

			if (!_inner.IsCancellationRequested)
				_cancel.Cancel();

			return _inner.IsCancellationRequested;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					_lock.Dispose();
					_cancel.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		~MyTaskSource() => Dispose(disposing: false);
	}
}