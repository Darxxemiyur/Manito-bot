using System;
using System.Text.Json;

namespace Manito.System.Logging
{
	public class LogLine : IDisposable
	{
		public long ID {
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

		public LogLine(string district, string category, JsonDocument data)
		{
			District = district;
			Category = category;
			Data = data;
		}

		public void Dispose() => Data?.Dispose();
	}
}