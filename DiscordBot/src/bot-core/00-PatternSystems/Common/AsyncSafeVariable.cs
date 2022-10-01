using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
		private readonly SemaphoreSlim _sync;
		public AsyncSafeVariable() : this(default) { }
		public AsyncSafeVariable(T value)
		{
			_value = value;
			_sync = new(1, 1);
		}
		public async Task SetValue(T value)
		{
			await _sync.WaitAsync();
			_value = value;
			await Task.Run(_sync.Release);
		}
		public async Task SetValue(Func<T, Task<T>> value)
		{
			await _sync.WaitAsync();
			_value = await value(_value);
			await Task.Run(_sync.Release);
		}
		public async Task<T> GetValue()
		{
			await _sync.WaitAsync();
			var val = _value;
			await Task.Run(_sync.Release);
			return val;
		}
		public async Task<T> GetValue(Func<T, Task<T>> value)
		{
			await _sync.WaitAsync();
			var val = await value(_value);
			await Task.Run(_sync.Release);
			return val;
		}
		public static implicit operator T(AsyncSafeVariable<T> val) => val._value;
	}
}
