
using Name.Bayfaderix.Darxxemiyur.Common;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Manito.Discord.Orders
{
	public class Order
	{
		private List<Step> _steps;
		public IReadOnlyList<Step> Steps => _steps;
		public Order(params Step[] steps) => _steps = steps?.ToList() ?? new();
		public ulong Initiator;
		public ulong OrderId = OrderIds++;
		private static ulong OrderIds = 0;
		private readonly TaskCompletionSource<string> _handle = new();
		private readonly CancellationTokenSource _cancel = new();
		public Task<string> OrderFinishTask;
		private readonly AsyncLocker _lock = new();
		private bool _isNotCancellable;
		public void SetSteps(IEnumerable<Step> steps) => _steps = steps.ToList();
		public void SetSteps(params Step[] steps) => _steps = steps.ToList();
		public async Task CancelOrder()
		{
			using var ff = await _lock.BlockAsyncLock();

			if (_isNotCancellable)
				return;

			await Task.Run(_cancel.Cancel);
		}
		public async Task MakeUncancellable()
		{
			using var ff = await _lock.BlockAsyncLock();

			_isNotCancellable = true;
		}

		public Task FinishOrder(string message) => Task.FromResult(_handle.TrySetResult(message));



		public abstract class Step
		{
			public abstract StepType Type {
				get;
			}
		}
		public class ConfirmationStep : Step
		{
			public ConfirmationStep(ulong userId, string description, string question)
			{
				UserId = userId;
				Description = description;
				Question = question;
			}
			public override StepType Type => StepType.Confirmation;
			public ulong UserId {
				get;
			}
			public string Description {
				get;
			}
			public string Question {
				get;
			}
		}
		public class CommandStep : Step
		{
			public CommandStep(ulong userId, string description, string command)
			{
				UserId = userId;
				Description = description;
				Command = command;
			}
			public override StepType Type => StepType.Command;
			public ulong UserId {
				get;
			}
			public string Description {
				get;
			}
			public string Command {
				get;
			}
		}
		public class ShowInfoStep : Step
		{
			public ShowInfoStep(string description) => Description = description;
			public override StepType Type => StepType.ShowInfo;
			public string Description {
				get;
			}
		}
		public class ChangeStateSteop : Step
		{
			public override StepType Type {
				get;
			}
		}
		public enum StepType
		{
			Confirmation,
			Command,
			ShowInfo,
			ChangeState,
		}
	}
}
