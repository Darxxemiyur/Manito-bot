using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Emzi0767.Utilities;
using System.Linq;
using System.Threading;

namespace Manito.Discord.Client
{
    public class ExecThread
    {
        private List<Task> _executingTasks;
        private SemaphoreSlim _sync;
        private TaskCompletionSource _onNew;
        public ExecThread()
        {
            _sync = new(1, 1);
            _executingTasks = new List<Task>();
            _onNew = new();
        }
        public async Task AddNew(Func<Task> runner)
        {

            await _sync.WaitAsync();
            _executingTasks.Add(runner());
            _onNew.SetResult();
            _sync.Release();
        }
        public async Task Run()
        {
            while (true)
            {
                await _sync.WaitAsync();
                var list = _executingTasks.Append(_onNew.Task).ToArray();
                _sync.Release();

                var completedTask = await Task.WhenAny(list);

                await _sync.WaitAsync();
                _executingTasks.Remove(completedTask);
                _onNew = new();
                _sync.Release();
            }
        }
    }
}