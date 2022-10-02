
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
		public Order(ulong initiator, params Step[] steps) =>
			(Initiator, _steps) = (initiator, steps?.ToList() ?? new());
		public readonly ulong Initiator;
		public readonly ulong OrderId = OrderIds++;
		private static ulong OrderIds = 1;
		private readonly TaskCompletionSource OrderCancelled = new();
		private readonly TaskCompletionSource OrderNonCancellable = new();
		private readonly TaskCompletionSource OrderComplete = new();
		/// <summary>
		/// On order cancelled. True if cancelled by admin, false if by customer.
		/// </summary>
		public Task OrderCancelledTask => OrderCancelled.Task;
		private readonly CancellationTokenSource _playerOrderCancellation = new();
		public CancellationToken PlayerOrderCancelToken => _playerOrderCancellation.Token;
		private readonly CancellationTokenSource _adminOrderCancellation = new();
		public CancellationToken AdminOrderCancelToken => _adminOrderCancellation.Token;
		public Task OrderNonCancellableTask => OrderNonCancellable.Task;
		public Task OrderCompleteTask => OrderComplete.Task;
		private readonly AsyncLocker _lock = new();
		private bool _isNotCancellable;
		public void SetSteps(IEnumerable<Step> steps) => _steps = steps.ToList();
		public void SetSteps(params Step[] steps) => _steps = steps.ToList();
		/// <summary>
		/// Order cancellation by admin.
		/// </summary>
		public async Task CancelOrder()
		{
			await using var _ = await _lock.BlockAsyncLock();

			await Task.Run(_adminOrderCancellation.Cancel);
			await Task.Run(OrderCancelled.TrySetResult);
			await Task.Run(OrderComplete.TrySetCanceled);
		}
		/// <summary>
		/// Order cancellation by player
		/// </summary>
		/// <returns></returns>
		public async Task TryCancelOrder()
		{
			await using var _ = await _lock.BlockAsyncLock();

			if (_isNotCancellable)
				return;

			await Task.Run(_playerOrderCancellation.Cancel);
			await Task.Run(OrderCancelled.TrySetResult);
			await Task.Run(OrderComplete.TrySetCanceled);
		}
		public async Task FinishOrder()
		{
			await using var _ = await _lock.BlockAsyncLock();

			await Task.Run(OrderCancelled.TrySetCanceled);
			await Task.Run(OrderComplete.TrySetResult);
		}
		public async Task MakeUncancellable()
		{
			await using var _ = await _lock.BlockAsyncLock();

			_isNotCancellable = true;
			await Task.Run(OrderNonCancellable.SetResult);
		}

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
			public override StepType Type => StepType.ChangeState;
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
