using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Name.Bayfaderix.Darxxemiyur.Common
{
	public class MyRelayTask
	{
		private MyRelayTask<bool> _facade;
		public MyRelayTask(Task work, CancellationToken token = default) : this(() => work, token) { }
		public MyRelayTask(Func<Task> work, CancellationToken token = default) => _facade = new(async () => { await work(); return false; }, token);
		public Task TheTask => _facade.TheTask;
	}
	public class MyRelayTask<T>
	{
		private MyTaskSource<T> _inner;
		private Task<T> _innerWork;
		private Func<Task<T>> _callable;
		private AsyncLocker _lock;
		public Task<T> TheTask => Encapsulate();
		private MyRelayTask(CancellationToken token = default)
		{
			_inner = new(token);
			_lock = new();
		}
		public MyRelayTask(Task<T> work, CancellationToken token = default) : this(() => work, token) { }
		public MyRelayTask(Func<Task<T>> work, CancellationToken token = default) : this(token) => _callable = work;
		private async Task<T> Encapsulate()
		{
			{
				await using var _ = await _lock.BlockAsyncLock();
				_innerWork ??= SecureThingy();
			}
			return await _innerWork;
		}
		private async Task<T> SecureThingy()
		{
			var task = _callable();

			var either = await Task.WhenAny(task, _inner.MyTask);

			if (either == task)
				await _inner.TrySetResultAsync(await task);
			else
				await _inner.TrySetCanceledAsync();

			return await _inner.MyTask;
		}
	}
}
