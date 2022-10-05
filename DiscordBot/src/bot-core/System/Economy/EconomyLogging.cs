using Manito.Discord;
using Manito.Discord.Client;

using Name.Bayfaderix.Darxxemiyur.Common;

using System.Threading.Tasks;

namespace Manito.System.Economy
{
	public class EconomyLogging : IModule
	{
		#region ToRework

		private ulong logid = 973271532681982022;
		private TaskEventProxy<string> _logQueue = new();
		private MyDomain _service;

		public EconomyLogging(MyDomain service)
		{
			_service = service;
		}

		public Task ReportTransaction(string str) => _logQueue.Handle(str);

		public Task RunModule() => LogTransactions();

		private async Task LogTransactions()
		{
			while (true)
			{
				var str = await _logQueue.GetData();
				var ch = await _service.MyDiscordClient.Client.GetChannelAsync(logid);
				await ch.SendMessageAsync(str);
			}
		}

		#endregion ToRework
	}
}