using System;
using Microsoft.EntityFrameworkCore;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using Manito.Discord.Database;
using System.Threading.Tasks;
using Manito.Discord.Client;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus;
using Manito.Discord.Chat.DialogueNet;
using Name.Bayfaderix.Darxxemiyur.Node.Network;
using DSharpPlus.EventArgs;

namespace Manito.Discord.PermanentMessage
{

	public class MsgWallFilter : IModule
	{
		private MyDomain _domain;
		private List<DiscordApplicationCommand> _commandList;
		private DiscordEventProxy<DiscordInteraction> _queue;
		public MsgWallFilter(MyDomain domain, EventBuffer buffer)
		{
			_domain = domain;
			_queue = new();
			_commandList = GetCommands().ToList();
			domain.MyDiscordClient.AppCommands.Add("MsgControll", _commandList);
			buffer.Interact.OnMessage += FilterMessage;
		}
		private IEnumerable<DiscordApplicationCommand> GetCommands()
		{
			yield return new DiscordApplicationCommand("messagewall", "Редактировать стену", null, true);
		}

		private async Task FilterMessage(DiscordClient client, InteractionCreateEventArgs args)
		{
			try
			{
				if (!_commandList.Any(x => args.Interaction.Data.Name.Contains(x.Name)))
					return;
				var guild = await _domain.MyDiscordClient.ManitoGuild;

				var user = await guild.GetMemberAsync(args.Interaction.User.Id);

				if (!user.Permissions.HasPermission(Permissions.Administrator))
					return;
			}
			catch
			{
				return;
			}

			await _queue.Handle(client, args.Interaction);
			args.Handled = true;
		}
		public async Task RunModule()
		{
			while (true)
			{
				var data = (await _queue.GetData()).Item2;
				await HandleAsCommand(data);
			}
		}
		private async Task HandleAsCommand(DiscordInteraction args)
		{
			await _domain.MsgWallCtr.StartSession(args);
		}
	}
}
