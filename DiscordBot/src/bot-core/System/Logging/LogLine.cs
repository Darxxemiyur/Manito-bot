using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
