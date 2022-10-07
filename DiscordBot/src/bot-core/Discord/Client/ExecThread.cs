using Name.Bayfaderix.Darxxemiyur.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Manito.Discord.Client
{
	public class ExecThread : IModule
	{
		private readonly List<Task<Exception>> _executingTasks;
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
		private async Task<Exception> SafeHandler(Func<Task> invoke)
		{
			try
			{
				await invoke();
			}
			catch (Exception e)
			{
				return e;
			}
			return null;
		}
		public async Task RunModule()
		{
			while (true)
			{
				{
					// Handle the add queue
					await using var _ = await _sync.BlockAsyncLock();
					_executingTasks.AddRange(_toExecuteTasks.Select(x => SafeHandler(x)));
					_toExecuteTasks.Clear();
				}
				//Wait for any task to complete in the list;
				var completedTask = await Task.WhenAny(_executingTasks.Append(_onNew.Task).ToArray()) as Task<Exception>;
				if (completedTask != null)
				{
					//Handle the removal of completed tasks yielded from awaiting for any
					await using var _ = await _sync.BlockAsyncLock();
					var result = await completedTask;
					//Forward all exceptions to the stderr-ish
					if (result != null)
						await Console.Error.WriteLineAsync($"{result}");
					//await completedTask;

					//Returns false if it tries to remove 'timeout' task, and true if succeeds
					_executingTasks.Remove(completedTask);
					_onNew = new();
				}
			}
		}
	}
}