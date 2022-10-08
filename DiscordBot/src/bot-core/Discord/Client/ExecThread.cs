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
		private MyDomain _domain;

		public ExecThread(MyDomain domain)
		{
			_sync = new();
			_executingTasks = new();
			_toExecuteTasks = new();
			_onNew = new();
			_domain = domain;
		}

		public async Task<Task<Task>> AddNew(Func<Task> runner) => (await AddNew(new[] { runner })).First();

		public Task<IEnumerable<Task<Task>>> AddNew(params Func<Task>[] runners) => AddNew(runners.AsEnumerable());
		/// <summary>
		/// Returns a list of tasks that represent process of passed tasks, which on completion will return the completed tasks;
		/// </summary>
		/// <param name="runners"></param>
		/// <returns></returns>
		public async Task<IEnumerable<Task<Task>>> AddNew(IEnumerable<Func<Task>> runners)
		{
			using var g = await _sync.BlockAsyncLock();
			var runner = runners.Select(SafeRelayHandler);
			_toExecuteTasks.AddRange(runner.Select(x => x.Item1));
			_onNew.TrySetResult();
			return runner.Select(x => x.Item2);
		}
		private (Func<Task>, Task<Task>) SafeRelayHandler(Func<Task> invoke)
		{
			var relay = new TaskCompletionSource<Task>();
			var newInvoke = async () => {
				Task invT = null;
				try
				{
					invT = invoke();
					await invT;
				}
				finally
				{
					relay.SetResult(invT);
				}
			};
			return (newInvoke, relay.Task);
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
					{
						await Console.Error.WriteLineAsync($"{result}");
						await _domain.Logging.WriteErrorClassedLog(GetType().Name, $"{result}", false);
					}
					//Returns false if it tries to remove 'timeout' task, and true if succeeds
					_executingTasks.Remove(completedTask);
					_onNew = new();
				}
			}
		}
	}
}