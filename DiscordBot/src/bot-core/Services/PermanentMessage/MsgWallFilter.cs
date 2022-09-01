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
using Cyriller;

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
			buffer.ContInteract.OnMessage += FilterMessage;
		}
		private const string Locale = "ru";
		private Dictionary<string, string> GetLoc(string trans) => new() { { Locale, trans } };
		private IEnumerable<DiscordApplicationCommand> GetCommands()
		{
			yield return new DiscordApplicationCommand("msgwall", "Edit wall", null, true,
				ApplicationCommandType.SlashCommand, GetLoc("стенасооб"), GetLoc("Редактировать стены"));
			yield return new DiscordApplicationCommand("Message wall import", "", null, true,
				ApplicationCommandType.MessageContextMenu, GetLoc("Импорт сообщения в строку стены"));
		}

		private async Task FilterMessage(DiscordClient client, InteractionCreateEventArgs args)
		{
			if (!_commandList.Any(x => args.Interaction.Data.Name.Contains(x.Name)))
				return;

			if (!await IsWorthy(args.Interaction))
				return;

			await _queue.Handle(client, args.Interaction);
			args.Handled = true;
		}
		private async Task FilterMessage(DiscordClient client, ContextMenuInteractionCreateEventArgs args)
		{
			if (!_commandList.Any(x => args.Interaction.Data.Name.Contains(x.Name)))
				return;

			if (!await IsWorthy(args.Interaction))
				return;

			await _queue.Handle(client, args.Interaction);
			args.Handled = true;
		}
		private Task<bool> IsWorthy(DiscordInteraction interaction) => IsWorthy(interaction.User);
		private Task<bool> IsWorthy(DiscordUser user) => IsWorthy(user.Id);
		private async Task<bool> IsWorthy(ulong id)
		{
			try
			{
				var guild = await _domain.MyDiscordClient.ManitoGuild;

				var user = await guild.GetMemberAsync(id);

				return user.Permissions.HasPermission(Permissions.Administrator);
			}
			catch { return false; }
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
			if (args.Data.Type == ApplicationCommandType.SlashCommand)
				await _domain.MsgWallCtr.StartSession(args);

			if (args.Data.Type == ApplicationCommandType.MessageContextMenu)
				await ImportMessage(args);
		}
		private async Task ImportMessage(DiscordInteraction args)
		{
			var msg = args.Data.Resolved.Messages.First().Value;

			_domain.MsgWallCtr.ImportedMessages.Add(new ImportedMessage {
				Message = msg.Embeds.FirstOrDefault()?.Description ??
				(msg.Content.IsNullOrEmpty() ? "" : msg.Content),
				MessageId = msg.Id,
				UserId = args.User.Id
			});

			await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
				new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder().
				WithDescription(msg.Content).WithFooter("Успешно импортировано!")).AsEphemeral(true));
		}
	}
}
