using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using DSharpPlus.SlashCommands.Attributes;

using Manito.Discord.Client;
using Manito.Discord.Economy;

namespace Manito.Discord.Economy
{

	public class DebugFilter : IModule
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
		private DebugCommands _commands;
		private DiscordEventProxy<InteractionCreateEventArgs> _queue;
		private MyDomain _service;
		public DebugFilter(MyDomain service, EventBuffer eventBuffer)
		{
			_commands = new DebugCommands(service);
			service.MyDiscordClient.AppCommands.Add("Debug", _commands.GetCommands());
			_queue = new();
			_service = service;
			eventBuffer.Interact.OnMessage += _queue.Handle;
		}
		public async Task FilterMessage(DiscordClient client, InteractionCreateEventArgs args)
		{
			try
			{
				var res = _commands.Search(args.Interaction);
				if (res == null)
					return;

				var guild = await _service.MyDiscordClient.ManitoGuild;

				var user = await guild.GetMemberAsync(args.Interaction.User.Id);

				if (!user.Permissions.HasPermission(Permissions.Administrator))
					return;

				await res(args.Interaction);
				args.Handled = true;
			}
			catch { }
		}
	}

}