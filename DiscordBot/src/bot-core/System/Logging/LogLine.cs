using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Manito.System.Logging
{
	public class LogLine : IDisposable
	{
		public ulong Id {
			get; set;
		}
		public string District {
			get; set;
		}
		public string Category {
			get; set;
		}
		public JsonDocument Data {
			get; set;
		}
		public void Dispose() => Data?.Dispose();
	}
}
