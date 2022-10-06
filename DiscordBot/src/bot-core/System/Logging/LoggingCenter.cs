using DisCatSharp;
using DisCatSharp.EventArgs;

using Manito.Discord.Client;

using Name.Bayfaderix.Darxxemiyur.Common;

using Newtonsoft.Json;

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using JsonException = System.Text.Json.JsonException;

namespace Manito.System.Logging
{
	public class LoggingCenter : IModule
	{
		private readonly AsyncLocker _locker = new();
		private readonly MyDiscordClient _client;
		private readonly ILoggingDBFactory _factory;
		private readonly TaskEventProxy<(string, string)> _relay;

		public LoggingCenter(MyDiscordClient client, ILoggingDBFactory factory)
		{
			(_client, _factory) = (client, factory);
			var dc = client.Client;
			_relay = new();
			dc.PayloadReceived += Dc_PayloadReceived;
		}

		private async Task Dc_PayloadReceived(DiscordClient sender, PayloadReceivedEventArgs e)
		{
			await _client.Domain.ExecutionThread.AddNew(() => _relay.Handle(("DiscordBotLog", e.Json)));
		}

		private async Task<(bool, JsonDocument)> TryParseAsync(string log)
		{
			try
			{
				var stream = new MemoryStream();
				var writer = new StreamWriter(stream);
				writer.Write(log);
				writer.Flush();
				stream.Position = 0;
				return (true, await JsonDocument.ParseAsync(stream));
			}
			catch (Exception e) when (e is ArgumentException or JsonException)
			{
				return (false, null);
			}
		}

		public async Task WriteLog(string district, string log)
		{
			var (res, jlog) = await TryParseAsync(log);
			if (!res)
			{
				var convertedLog = JsonConvert.SerializeObject(new
				{
					type = "ManuallyConvertedDueToNotBeingJsonInTheFirstPlace",
					data = log
				});
				await WriteLog(district, convertedLog);
				return;
			}

			await using var _ = await _locker.BlockAsyncLock();
			await using var db = await _factory.CreateLoggingDBContextAsync();

			db.LogLines.Add(new LogLine("Discord", district, jlog));

			await db.SaveChangesAsync();
		}

		public async Task RunModule()
		{
			while (true)
			{
				var (district, log) = await _relay.GetData();
				await _client.Domain.ExecutionThread.AddNew(() => WriteLog(district, log));
			}
		}
	}
}