
using Name.Bayfaderix.Darxxemiyur.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Manito.Discord.Orders
{
	public class AdminOrderPool
	{
		public int AdminsOnline {
			get;
			private set;
		}
		public bool AnyAdminOnline => AdminsOnline > 0;
		private readonly AsyncLocker _lock;
		private readonly PoolTaskEventProxy _pool;
		public AdminOrderPool()
		{
			AdminsOnline = 0;
			_lock = new();
			_pool = new();
		}
		public async Task StartAdministrating()
		{
			await using var _ = await _lock.BlockAsyncLock();

			AdminsOnline += 1;
		}
		public async Task StopAdministrating()
		{
			await using var _ = await _lock.BlockAsyncLock();

			AdminsOnline = Math.Max(0, AdminsOnline - 1);

			while (!AnyAdminOnline && await _pool.AnyOrders())
			{
				var order = await await _pool.GetOrder();
				await order.CancelOrder("Последний администратор ушёл с поста. Средства возвращены.");
			}
		}
		public async Task<bool> IsAnyAdminOnline()
		{
			await using var _ = await _lock.BlockAsyncLock();
			return AnyAdminOnline;
		}
		public async Task<bool> PlaceOrder(Order order)
		{
			await using var _ = await _lock.BlockAsyncLock();
			if (AnyAdminOnline)
				await _pool.PlaceOrder(order);
			else
				await order.CancelOrder("Администраторов в сети нет.");

			return AnyAdminOnline;
		}
		public async Task<Order> GetOrder(CancellationToken token = default)
		{
			var order = Task.FromResult<Order>(null);
			{
				await using var _ = await _lock.BlockAsyncLock();
				order = await _pool.GetOrder(token);
			}
			return await order;
		}
	}
}
