using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.EventArgs;
using DisCatSharp.ApplicationCommands.Attributes;

using Manito.Discord.Client;
using Name.Bayfaderix.Darxxemiyur.Common;
using Manito.System.Economy; using Manito.Discord;

namespace Manito.System.Economy
{

	public class EconomyFilter : IModule
	{

		public Task RunModule() => HandleLoop();
		private async Task HandleLoop()
		{
			while (true)
			{
				var data = await _queue.GetData();
				await FilterMessage(data.Item1, data.Item2);
			}
		}
		private EconomyCommands _commands;
		private DiscordEventProxy<InteractionCreateEventArgs> _queue;
		private MyDomain _domain;
		public EconomyFilter(MyDomain service, EventBuffer eventBuffer)
		{
			_commands = new EconomyCommands(service.Economy, service.MyDiscordClient);
			(_domain = service).MyDiscordClient.AppCommands.Add("Economy", _commands.GetCommands());
			_queue = new();
			eventBuffer.Interact.OnMessage += _queue.Handle;
		}
		public async Task FilterMessage(DiscordClient client, InteractionCreateEventArgs args)
		{
			var res = _commands.Search(args.Interaction);
			if (res == null)
				return;

			await _domain.ExecutionThread.AddNew(() => res(args.Interaction));
			args.Handled = true;
		}
	}

}