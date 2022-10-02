﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Name.Bayfaderix.Darxxemiyur.Common
{
	public class TaskEventProxy<T> : IDisposable
	{
		public TaskEventProxy()
		{
			_sync = new();
			_chain = new();
			_chain.Enqueue((_generator = new TaskCompletionSource<T>()).Task);
		}
		private TaskCompletionSource<T> _generator;
		private readonly Queue<Task<T>> _chain;
		private readonly AsyncLocker _sync;
		public Task<bool> HasAny() => Task.FromResult(_chain.Any(x => x.IsCompleted));
		public async Task Handle(T stuff)
		{
			using var _ = await _sync.BlockAsyncLock();

			if (_generator.Task.IsCanceled)
				throw _generator.Task.Exception;

			_generator.SetResult(stuff);
			_chain.Enqueue((_generator = new TaskCompletionSource<T>()).Task);
		}
		public async Task Cancel()
		{
			using var _ = await _sync.BlockAsyncLock();
			_generator.SetCanceled();
		}
		public async Task<T> GetData(CancellationToken token = default)
		{
			Task<T> result = null;
			{
				using var _ = await _sync.BlockAsyncLock();
				result = _chain.Dequeue();
			}
			var canceller = Task.Run(() => result, token);
			var first = await Task.WhenAny(result, canceller);

			if (first == canceller && token.IsCancellationRequested)
			{
				_chain.Enqueue(result);
				await canceller;
			}

			return await result;
		}

		private bool disposedValue;
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects)
				}
			   ((IDisposable)_sync).Dispose();
				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				disposedValue = true;
			}
		}

		// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~TaskEventProxy()
		// {
		//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		~TaskEventProxy()
		{
			// Do not re-create Dispose clean-up code here.
			// Calling Dispose(false) is optimal in terms of
			// readability and maintainability.
			this.Dispose(false);
		}
	}
	public class TaskEventProxy : IDisposable
	{
		public TaskEventProxy()
		{
			_facade = new();
		}
		private TaskEventProxy<bool> _facade;
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
					// TODO: dispose managed state (managed objects)
				}
			   ((IDisposable)_facade).Dispose();
				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				disposedValue = true;
			}
		}

		// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~TaskEventProxy()
		// {
		//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		~TaskEventProxy()
		{
			// Do not re-create Dispose clean-up code here.
			// Calling Dispose(false) is optimal in terms of
			// readability and maintainability.
			this.Dispose(false);
		}
	}
}
