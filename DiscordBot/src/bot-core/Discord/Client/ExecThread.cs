using Name.Bayfaderix.Darxxemiyur.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Manito.Discord.Client
{
	public class ExecThread
	{
		private readonly List<Task> _executingTasks;
		private readonly List<Func<Task>> _toExecuteTasks;
		private readonly AsyncLocker _sync;
		private TaskCompletionSource _onNew;

		public ExecThread()
		{
			_sync = new();
			_executingTasks = new();
			_toExecuteTasks = new();
			_onNew = new();
		}

		public async Task AddNew(Func<Task> runner)
		{
			using var g = await _sync.BlockAsyncLock();
			_toExecuteTasks.Add(runner);
			_onNew.TrySetResult();
		}

		public async Task Run()
		{
			while (true)
			{
				// Handle the add queue
				await _sync.AsyncLock();
				_executingTasks.AddRange(_toExecuteTasks.Select(x => x()));
				_toExecuteTasks.Clear();
				var list = _executingTasks.Append(_onNew.Task).ToArray();
				await _sync.AsyncUnlock();

				//Wait for any task to complete in the list;
				var completedTask = await Task.WhenAny(list);

				//Handle the removal of completed tasks yielded from awaiting for any
				await _sync.AsyncLock();

				//Forward all exceptions to the stderr-ish
				if (completedTask?.Exception != null)
					await Console.Error.WriteLineAsync($"{completedTask.Exception}");
				//await completedTask;

				//Returns false if it tries to remove 'timeout' task, and true if succeeds
				_executingTasks.Remove(completedTask);
				_onNew = new();
				await _sync.AsyncUnlock();
			}
		}
	}
}