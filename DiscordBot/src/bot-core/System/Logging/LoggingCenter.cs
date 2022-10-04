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
	public class LoggingCenter
	{
		private readonly AsyncLocker _locker = new();
		private readonly MyDiscordClient _client;
		private readonly ILoggingDBFactory _factory;
		public LoggingCenter(MyDiscordClient client, ILoggingDBFactory factory)
		{
			(_client, _factory) = (client, factory);
			var dc = client.Client;
			dc.PayloadReceived += Dc_PayloadReceived;
		}

		private async Task Dc_PayloadReceived(DisCatSharp.DiscordClient sender, DisCatSharp.EventArgs.PayloadReceivedEventArgs e)
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(e.Json);
			writer.Flush();
			stream.Position = 0;
			var log = await JsonDocument.ParseAsync(stream);

			await _client.Domain.ExecutionThread.AddNew(() => WriteLog("DiscordBotLog", log));
		}
		public async Task WriteLog(string district, JsonDocument body)
		{
			await using var _ = await _locker.BlockAsyncLock();
			var db = await _factory.CreateLoggingDBContextAsync();

			db.LogLines.Add(new LogLine { Category = "Discord", District = district, Data = body });

			await db.SaveChangesAsync();
		}
	}
}
