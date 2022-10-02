using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Name.Bayfaderix.Darxxemiyur.Common
{
	//TODO: Make locker wait to be disposed, until all awaiters go out.
	public sealed class AsyncLocker : IDisposable
	{
		private readonly SemaphoreSlim _lock;
		public AsyncLocker() => _lock = new(1, 1);
		public Task AsyncLock(CancellationToken token = default) => _lock.WaitAsync(token);
		public void Lock(CancellationToken token = default) => _lock.Wait(token);
		public Task AsyncLock(TimeSpan time, CancellationToken token = default) => _lock.WaitAsync(time, token);
		public void Lock(TimeSpan time, CancellationToken token = default) => _lock.Wait(time, token);
		public Task<BlockAsyncLock> BlockAsyncLock(CancellationToken token = default) =>
			AsyncLock(token).ContinueWith((x) => new BlockAsyncLock(this));
		public BlockAsyncLock BlockLock()
		{
			Lock();
			return new BlockAsyncLock(this);
		}
		public Task AsyncUnlock() => Task.Run(Unlock);
		public void Unlock() => _lock.Release();
		public void Dispose() => ((IDisposable)_lock).Dispose();
	}
	public sealed class BlockAsyncLock : IDisposable, IAsyncDisposable
	{
		private readonly AsyncLocker _lock;
		public BlockAsyncLock(AsyncLocker tlock) => _lock = tlock;
		public void Dispose() => _lock.Unlock();
		public ValueTask DisposeAsync() => new(_lock.AsyncUnlock());
	}
}
