using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Manito._00_PatternSystems.Common
{
	/// <summary>
	/// Variable that is safe to access and set in Async workflow
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class AsyncSafeVariable<T>
	{
		private T _value;
		private readonly SemaphoreSlim _sync;

		public async Task SetValue(T value)
		{
			await _sync.WaitAsync();
			_value = value;
			_sync.Release();
		}
		public async Task<T> GetValue()
		{
			await _sync.WaitAsync();
			var val = _value;
			_sync.Release();
			return val;
		}
	}
}
