using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;

using Manito.Discord.Client;
namespace Manito.Discord.Economy
{

	public class DebugCommands
	{
		private const string Locale = "ru";
		private MyDomain _bot;
		public DebugCommands(MyDomain dom) => _bot = dom;
		public Func<DiscordInteraction, Task> Search(DiscordInteraction command)
		{
			foreach (var item in GetCommands())
			{
				if (command.Data.Name.Contains(item.Name))
				{
					foreach (var subItem in GetSubCommands())
					{
						if (command.Data.Options.First().Name.Contains(subItem.Item1.Name))
							return subItem.Item2;
					}
				}
			}
			return null;
			/*
            return GetCommands().Where(x => command.Data.Name.Contains(x.Name))
            .SelectMany(x => GetSubCommands()).Distinct()
            .FirstOrDefault(x => command.Data.Options.First().Name.Contains(x.Item1.Name))
            .Item2;
            */
		}
		private Dictionary<string, string> GetLoc(string trans) => new Dictionary<string, string>() { { Locale, trans } };

		public IEnumerable<DiscordApplicationCommand> GetCommands()
		{
			yield return new DiscordApplicationCommand("debug", "Debug",
			 GetSubCommands().Select(x => x.Item1), true,
			 ApplicationCommandType.SlashCommand,
			 GetLoc("дебаг"),
			 GetLoc("Дебаг"));

		}
		private IEnumerable<(DiscordApplicationCommandOption, Func<DiscordInteraction, Task>)> GetSubCommands()
		{
			yield return (new DiscordApplicationCommandOption("test_time", "Test time",
			 ApplicationCommandOptionType.SubCommand, null, null, new[] {
				new DiscordApplicationCommandOption("time", "Time", ApplicationCommandOptionType.String,
				 false, name_localizations: GetLoc( "время"),
				 description_localizations: GetLoc( "Время")),
				new DiscordApplicationCommandOption("msg", "Msg", ApplicationCommandOptionType.String,
				 false, name_localizations: GetLoc( "msg"),
				 description_localizations: GetLoc( "Msg"))
			 },
			 name_localizations: GetLoc("проверить_время"),
			 description_localizations: GetLoc("Проверить формат времени")),
			 GetAccountDeposit);
			yield return (new DiscordApplicationCommandOption("reset_db", "Reset database",
			 ApplicationCommandOptionType.SubCommand, null, null, null,
			 name_localizations: GetLoc("сбросить_бд"),
			 description_localizations: GetLoc("Сбросить базу данных")),
			 ResetDatabase);
			yield return (new DiscordApplicationCommandOption("check_wm", "Check welcomming message",
			 ApplicationCommandOptionType.SubCommand, null, null, null,
			 name_localizations: GetLoc("проверить_пс"),
			 description_localizations: GetLoc("Проверить приветственное сообщение")),
			 CheckMessage);
		}
		private async Task CheckMessage(DiscordInteraction args)
		{
			var (guild, msgs) = await _bot.Welcomer.GetMsg(args.User.Id);
			await args.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

			foreach (var msg in msgs)
				await args.Channel.SendMessageAsync(msg);
		}
		private async Task ResetDatabase(DiscordInteraction args)
		{
			using var fdb = await _bot.DbFactory.CreateMyDbContextAsync();
			var db = fdb.ImplementedContext;

			await db.Database.EnsureDeletedAsync();
			await db.Database.EnsureCreatedAsync();

			await args.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
		}

		/// <summary>
		/// Get user's Account deposit.
		/// </summary>
		/// <returns></returns>
		private async Task GetAccountDeposit(DiscordInteraction args)
		{
			var tools = new AppCommandArgsTools(args);

			var timeString = tools.AddOptArg("time");
			var msgString = tools.AddOptArg("msg");

			DateTimeOffset time = DateTimeOffset.Now + TimeSpan.FromMinutes(10);
			if (tools.GetOptional().Any(x => x.Key == timeString) &&
			 !DateTimeOffset.TryParse(tools.GetStringArg(timeString, false), out time))
				return;

			var msg = new DiscordInteractionResponseBuilder(new DiscordMessageBuilder()
			 .WithEmbed(new DiscordEmbedBuilder()
			 .WithDescription($"<t:{time.ToUnixTimeSeconds()}:R>\n"
			  + $"<t:{time.ToUnixTimeSeconds()}>\n{time}\n{tools.GetStringArg(msgString, false)}")));

			await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);

		}
	}

}
