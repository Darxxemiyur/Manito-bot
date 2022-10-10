using Name.Bayfaderix.Darxxemiyur.Common;

using Nito.AsyncEx;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Manito.Discord.Client
{
	public class ExecThread : IModule
	{
		private readonly List<Task<Exception>> _executingTasks;
		private readonly List<Job> _toExecuteTasks;
		private readonly AsyncLocker _sync;
		private Thread _workerThread;
		private MyTaskSource _relay;
		private MyTaskSource _onNew;
		private MyDomain _domain;

		public class Job
		{
			private readonly MyTaskSource<object> _resulter;
			public Task<object> DataResult => _resulter.MyTask;
			public Task Result => _resulter.MyTask;

			public enum Type
			{
				Pooled,
				Threaded,
				Inline,
			}

			public Type JobType => Type.Inline;
			private readonly Func<CancellationToken, Task<object>> _invoke;
			private readonly CancellationToken _token;

			private Job(CancellationToken token = default)
			{
				_token = token;
				_resulter = new();
			}

			/// <summary>
			/// Cancellation tokens will be delivered to the supplied task.
			/// </summary>
			public Job(Func<CancellationToken, Task<object>> work, CancellationToken token = default) : this(token) => _invoke = work;

			/// <summary>
			/// Cancellation tokens will be delivered to the supplied task.
			/// </summary>
			public Job(Func<Task<object>> work, CancellationToken token = default) : this(token) => _invoke = (CancellationToken x) => work();

			/// <summary>
			/// Cancellation tokens will be delivered to the supplied task.
			/// </summary>
			public Job(Func<CancellationToken, Task> work, CancellationToken token = default) : this(token) => _invoke = async (CancellationToken x) => {
				await work(x);
				return false;
			};

			/// <summary>
			/// Cancellation tokens will be delivered to the supplied task.
			/// </summary>
			public Job(Func<Task> work, CancellationToken token = default) : this(token) => _invoke = async (CancellationToken x) => {
				await work();
				return false;
			};

			public async Task Launch()
			{
				try
				{
					await _resulter.TrySetResultAsync(await _invoke(_token));
				}
				catch (TaskCanceledException)
				{
					await _resulter.TrySetCanceledAsync();
					throw;
				}
				catch (Exception e)
				{
					await _resulter.TrySetExceptionAsync(e);
					throw;
				}
			}
		}

		public ExecThread(MyDomain domain)
		{
			_sync = new();
			_executingTasks = new();
			_toExecuteTasks = new();
			_workerThread = new(() => AsyncContext.Run(MyWorker));
			_onNew = new();
			_relay = new();
			_domain = domain;
		}

		/// <summary>
		/// Returns a task that represent process of passed task, which on completion will return
		/// the completed task;
		/// </summary>
		/// <param name="runners"></param>
		/// <returns></returns>
		public async Task<Job> AddNew(Job job) => (await AddNew(new[] { job })).First();

		/// <summary>
		/// Returns a list of tasks that represent process of passed tasks, which on completion will
		/// return the completed tasks;
		/// </summary>
		/// <param name="runners"></param>
		/// <returns></returns>
		public Task<IEnumerable<Job>> AddNew(params Job[] runners) => AddNew(runners.AsEnumerable());

		/// <summary>
		/// Returns a list of tasks that represent process of passed tasks, which on completion will
		/// return the completed tasks;
		/// </summary>
		/// <param name="runners"></param>
		/// <returns></returns>
		public async Task<IEnumerable<Job>> AddNew(IEnumerable<Job> runners)
		{
			using var g = await _sync.BlockAsyncLock();

			_toExecuteTasks.AddRange(runners);
			await _onNew.TrySetResultAsync();

			return runners;
		}

		private async Task<Exception> SafeHandler(Job job)
		{
			try
			{
				// Place task in thread pool
				if (job.JobType == Job.Type.Pooled)
					await Task.Run(job.Launch);

				//Explicit external thread for each task.
				if (job.JobType == Job.Type.Threaded)
				{
					var relay = new MyTaskSource();
					new Thread(() => AsyncContext.Run(async () => {
						try
						{
							await job.Launch();
							await relay.TrySetResultAsync();
						}
						catch (Exception e)
						{
							await relay.TrySetExceptionAsync(e);
						}
					})).Start();
					await relay.MyTask;
				}

				//No separeate threads.
				if (job.JobType == Job.Type.Inline)
					await job.Launch();
			}
			catch (Exception e)
			{
				return e;
			}
			return null;
		}

		private async Task MyWorker()
		{
			try
			{
				while (true)
				{
					{
						// Handle the add queue
						await using var _ = await _sync.BlockAsyncLock();
						_executingTasks.AddRange(_toExecuteTasks.Select(SafeHandler));
						_toExecuteTasks.Clear();
					}
					//Wait for any task to complete in the list;
					var completedTask = await Task.WhenAny(_executingTasks.Append(_onNew.MyTask).ToArray()) as Task<Exception>;
					if (completedTask != null)
					{
						//Handle the removal of completed tasks yielded from awaiting for any
						await using var _ = await _sync.BlockAsyncLock();
						var result = await completedTask;
						//Forward all exceptions to the stderr-ish
						if (result != null)
							await _domain.Logging.WriteErrorClassedLog(GetType().Name, result, false);

						//Returns false if it tries to remove 'timeout' task, and true if succeeds
						_executingTasks.Remove(completedTask);
						_onNew = new();
					}
				}
			}
			catch (Exception)
			{
				//Exit
			}
			await _relay.TrySetResultAsync();
		}

		public async Task RunModule()
		{
			_workerThread.Start();
			await _relay.MyTask;
		}
	}
}