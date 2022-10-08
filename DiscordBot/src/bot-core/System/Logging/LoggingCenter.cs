using DisCatSharp;
using DisCatSharp.EventArgs;

using Manito._00_PatternSystems.Common;
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
		private OPFIFOTACollection<LogLine> _queue;
		private readonly MyClientBundle _client;
		private readonly ILoggingDBFactory _factory;
		private readonly FIFOACollection<(string, string)> _relay;

		public LoggingCenter(MyClientBundle client, ILoggingDBFactory factory)
		{
			(_client, _factory) = (client, factory);
			var dc = client.Client;
			_relay = new();
			_queue = new();
			dc.PayloadReceived += Dc_PayloadReceived;
		}

		private Task Dc_PayloadReceived(DiscordClient sender, PayloadReceivedEventArgs e) => _client.Domain.ExecutionThread.AddNew(() => _relay.Handle(("DiscordBotLog", e.Json)));

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

		public Task WriteErrorClassedLog(string district, Exception err, bool isHandled) => WriteErrorClassedLog(district, $"{err}", isHandled);

		public async Task WriteErrorClassedLog(string district, string log, bool isHandled)
		{
			var jlog = await MakeJsonLog(log);
			var jflog = await MakeJsonLog(JsonConvert.SerializeObject(new
			{
				type = "error",
				dataType = "ManuallyConvertedDueToNotBeingJsonInTheFirstPlace",
				isHandled,
				data = jlog
			}));
			await InnerWriteLogToDB(district, jflog);
		}

		public Task WriteJsonLog(string district, JsonDocument log) => InnerWriteLogToDB(district, log);

		private async Task<JsonDocument> MakeJsonLog(string log)
		{
			var (res, jlog) = await TryParseAsync(log);
			if (!res)
			{
				var convertedLog = JsonConvert.SerializeObject(new
				{
					type = "ManuallyConvertedDueToNotBeingJsonInTheFirstPlace",
					data = log
				});
				return await MakeJsonLog(convertedLog);
			}
			return jlog;
		}

		private async Task InnerWriteLogToDB(string district, JsonDocument jlog)
		{
			await _queue.Place(new LogLine("Discord", district, jlog));
		}

		public async Task WriteLog(string district, string log) => await InnerWriteLogToDB(district, await MakeJsonLog(log));

		public Task RunModule() => Task.WhenAll(DiscordEventLogging(), RunDbLogging());

		private async Task DiscordEventLogging()
		{
			while (true)
			{
				var (district, log) = await _relay.GetData();
				await _client.Domain.ExecutionThread.AddNew(() => WriteLog(district, log));
			}
		}

		private async Task RunDbLogging()
		{
			while (true)
			{
				await _queue.UntilPlaced();
				await using var db = await _factory.CreateLoggingDBContextAsync();

				var range = await _queue.GetAll();
				await db.LogLines.AddRangeAsync(range);
				await db.SaveChangesAsync();

				foreach (var item in range)
					item.Dispose();
			}
		}
	}
}