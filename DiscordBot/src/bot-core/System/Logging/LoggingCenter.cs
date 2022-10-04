using DisCatSharp;
using DisCatSharp.EventArgs;

using Manito.Discord;
using Manito.Discord.Client;

using Name.Bayfaderix.Darxxemiyur.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
		public async Task WriteLog(string district, string log)
		{
			await using var _ = await _locker.BlockAsyncLock();
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(log);
			writer.Flush();
			stream.Position = 0;
			var jlog = await JsonDocument.ParseAsync(stream);

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
