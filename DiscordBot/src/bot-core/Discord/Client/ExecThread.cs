using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Common.Utilities;
using System.Linq;
using System.Threading;

namespace Manito.Discord.Client
{
	public class ExecThread
	{
		private readonly List<Task> _executingTasks;
		private readonly List<Func<Task>> _toExecuteTasks;
		private readonly SemaphoreSlim _sync;
		private TaskCompletionSource _onNew;
		public ExecThread()
		{
			_sync = new(1, 1);
			_executingTasks = new();
			_toExecuteTasks = new();
			_onNew = new();
		}
		public async Task AddNew(Func<Task> runner)
		{
			await _sync.WaitAsync();
			_toExecuteTasks.Add(runner);
			_onNew.TrySetResult();
			_sync.Release();
		}
		public async Task Run()
		{
			while (true)
			{
				// Handle the add queue
				await _sync.WaitAsync();
				_executingTasks.AddRange(_toExecuteTasks.Select(x => x()));
				_toExecuteTasks.Clear();
				var list = _executingTasks.Append(_onNew.Task).ToArray();
				_sync.Release();

				//Wait for any task to complete in the list;
				var completedTask = await Task.WhenAny(list);

				//Handle the removal of completed tasks yielded from awaiting for any
				await _sync.WaitAsync();

				//Forward all exceptions to the stderr-ish
				if (completedTask?.Exception != null)
					await Console.Error.WriteLineAsync($"{completedTask.Exception}");
				//await completedTask;

				//Returns false if it tries to remove 'timeout' task, and true if succeeds
				_executingTasks.Remove(completedTask);
				_onNew = new();
				_sync.Release();
			}
		}
	}
}