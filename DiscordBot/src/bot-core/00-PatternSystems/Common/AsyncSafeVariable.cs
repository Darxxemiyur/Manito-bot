using System;
using System.Threading.Tasks;

namespace Name.Bayfaderix.Darxxemiyur.Common
{
	/// <summary>
	/// Variable that is safe to access and set in Async workflow
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class AsyncSafeVariable<T>
	{
		private T _value;
		private readonly AsyncLocker _sync;

		public AsyncSafeVariable() : this(default)
		{
		}

		public AsyncSafeVariable(T value)
		{
			_value = value;
			_sync = new();
		}

		public async Task SetValue(T value)
		{
			await using var _ = await _sync.BlockAsyncLock();
			_value = value;
		}

		public async Task SetValue(Func<T, Task<T>> value)
		{
			await using var _ = await _sync.BlockAsyncLock();
			_value = await value(_value);
		}

		public async Task<T> GetValue()
		{
			await using var _ = await _sync.BlockAsyncLock();
			var val = _value;

			return val;
		}

		public async Task<T> GetValue(Func<T, Task<T>> value)
		{
			await using var _ = await _sync.BlockAsyncLock();
			var val = await value(_value);
			return val;
		}

		public static implicit operator T(AsyncSafeVariable<T> val) => val._value;
	}
}