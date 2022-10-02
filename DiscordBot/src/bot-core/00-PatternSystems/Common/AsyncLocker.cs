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
		public Task AsyncLock() => _lock.WaitAsync();
		public void Lock() => _lock.Wait();
		public Task<BlockAsyncLock> BlockAsyncLock() => AsyncLock()
			.ContinueWith((x) => new BlockAsyncLock(this));
		public BlockAsyncLock BlockLock()
		{
			Lock();
			return new BlockAsyncLock(this);
		}
		public Task AsyncUnlock() => Task.Run(Unlock);
		public ValueTask AsyncValueUnlock() => new(AsyncUnlock());
		public void Unlock() => _lock.Release();
		public void Dispose() => ((IDisposable)_lock).Dispose();
	}
	public sealed class BlockAsyncLock : IDisposable, IAsyncDisposable
	{
		private readonly AsyncLocker _lock;
		public BlockAsyncLock(AsyncLocker tlock) => _lock = tlock;
		public void Dispose() => _lock.Unlock();
		public ValueTask DisposeAsync() => _lock.AsyncValueUnlock();
	}
}
