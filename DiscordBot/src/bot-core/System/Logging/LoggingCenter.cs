﻿using DisCatSharp;
using DisCatSharp.EventArgs;

using Manito._00_PatternSystems.Common;
using Manito.Discord.Client;

using Name.Bayfaderix.Darxxemiyur.Common;

using Newtonsoft.Json;

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Manito.System.Logging
{
	public class LoggingCenter : IModule
	{
		private FIFOPTACollection<LogLine> _queue;
		private readonly MyClientBundle _client;
		private readonly ILoggingDBFactory _factory;
		private readonly FIFOFBACollection<(string, string)> _relay;

		public LoggingCenter(MyClientBundle client, ILoggingDBFactory factory)
		{
			(_client, _factory) = (client, factory);
			var dc = client.Client;
			_relay = new();
			_queue = new();

			dc.PayloadReceived += Dc_PayloadReceived;
		}

		private Task Dc_PayloadReceived(DiscordClient sender, PayloadReceivedEventArgs e) => _client.Domain.ExecutionThread.AddNew(new ExecThread.Job(() => _relay.Handle(("DiscordBotLog", e.Json))));

		public async Task WriteErrorClassedLog(string district, Exception err, bool isHandled)
		{
#if DEBUG
			await Console.Out.WriteLineAsync("!!!Exception " + (isHandled ? "safely handled" : "not handled") + $"\n{err}\n\n\n");
#endif
			await WriteClassedLog(district, new
			{
				type = "error",
				isHandled,
				data = new
				{
					exception = err,
					digested_log = err.ToString()
				}
			});
		}

		public async Task WriteClassedLog(string district, object log) => await _relay.Handle((district, await ConvertTo(new
		{
			type = "classedlog",
			dataType = "ManuallyConvertedDueToNotBeingJsonInTheFirstPlace",
			data = await GetFromJson(log)
		})));

		private Task InnerWriteLogToDB(string district, JsonDocument jlog) => _queue.Place(new LogLine("Discord", district, jlog));

		public async Task WriteLog(string district, object log) => await InnerWriteLogToDB(district, await ParseJsonDocument(log));

		private async Task<object> GetFromJson(object input)
		{
			if (input is not string json)
				return input;

			return await ConvertFrom(json) ?? input;
		}

		private async Task<string> GetToJson(object input)
		{
			if (input is not string json)
				return await ConvertTo(input);

			var obj = await ConvertFrom(json);
			return obj != null ? await GetToJson(obj) : json;
		}

		private Task<string> ConvertTo(object itm) => Task.Run(() => JsonConvert.SerializeObject(itm));

		private Task<object> ConvertFrom(string json) => Task.Run(() => JsonConvert.DeserializeObject(json));

		private async Task<JsonDocument> ParseJsonDocument(object jsono)
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			var json = await GetToJson(jsono);
			Console.WriteLine(json);
			writer.Write(json);
			writer.Flush();
			stream.Position = 0;
			return await JsonDocument.ParseAsync(stream);
		}

		public Task RunModule() => Task.WhenAll(DiscordEventLogging(), RunDbLogging());

		private async Task DiscordEventLogging()
		{
			while (true)
			{
				var (district, log) = await _relay.GetData();
				await WriteLog(district, log);
			}
		}

		private async Task RunDbLogging()
		{
			while (true)
			{
				try
				{
					await _queue.UntilPlaced();
					await using var db = await _factory.CreateLoggingDBContextAsync();

					var range = await _queue.GetAll();
					await db.LogLines.AddRangeAsync(range);
					await db.SaveChangesAsync();

					foreach (var item in range)
						item.Dispose();
				}
				catch { await Task.Delay(TimeSpan.FromSeconds(60)); }
			}
		}
	}
}