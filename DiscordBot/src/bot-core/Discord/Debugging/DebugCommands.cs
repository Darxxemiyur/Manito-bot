using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.EventArgs;
using DisCatSharp.Enums;

using Manito.Discord.ChatNew;
using Manito.Discord.Client;
namespace Manito.Discord.Economy
{

	public class DebugCommands
	{
		private const string Locale = "ru";
		private MyDomain _bot;
		public DebugCommands(MyDomain dom) => (_bot) = (dom);
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
		}
		private DiscordApplicationCommandLocalization GetLoc(string trans) => new DiscordApplicationCommandLocalization(new Dictionary<string, string>() { { Locale, trans } });

		public IEnumerable<DiscordApplicationCommand> GetCommands()
		{
			yield return new DiscordApplicationCommand("debug", "Debug",
			 GetSubCommands().Select(x => x.Item1),
			 ApplicationCommandType.ChatInput,
			 GetLoc("дебаг"),
			 GetLoc("Дебаг"));

		}
		private IEnumerable<(DiscordApplicationCommandOption, Func<DiscordInteraction, Task>)> GetSubCommands()
		{
			yield return (new DiscordApplicationCommandOption("test_time", "Test time",
			 ApplicationCommandOptionType.SubCommand, false, null, new[] {
				new DiscordApplicationCommandOption("time", "Time", ApplicationCommandOptionType.String,
				 false, nameLocalizations: GetLoc( "время"),
				 descriptionLocalizations: GetLoc( "Время")),
				new DiscordApplicationCommandOption("msg", "Msg", ApplicationCommandOptionType.String,
				 false, nameLocalizations: GetLoc( "msg"),
				 descriptionLocalizations: GetLoc( "Msg"))
			 },
			 nameLocalizations: GetLoc("проверить_время"),
			 descriptionLocalizations: GetLoc("Проверить формат времени")),
			 GetAccountDeposit);
			yield return (new DiscordApplicationCommandOption("reset_db", "Reset database",
			 ApplicationCommandOptionType.SubCommand,
			 nameLocalizations: GetLoc("сбросить_бд"),
			 descriptionLocalizations: GetLoc("Сбросить базу данных")),
			 ResetDatabase);
			yield return (new DiscordApplicationCommandOption("check_wm", "Check welcomming message",
			 ApplicationCommandOptionType.SubCommand,
			 nameLocalizations: GetLoc("проверить_пс"),
			 descriptionLocalizations: GetLoc("Проверить приветственное сообщение")),
			 CheckMessage);
			yield return (new DiscordApplicationCommandOption("check_ds", "Check dialogue system",
			 ApplicationCommandOptionType.SubCommand,
			 nameLocalizations: GetLoc("проверить_дс"),
			 descriptionLocalizations: GetLoc("Проверить диалоговую систему")),
			 CheckDialogue);
		}
		private async Task CheckDialogue(DiscordInteraction args)
		{
			try
			{
				var at = _bot.MyDiscordClient.ActivityTools;

				var rs = new ComponentDialogueSession(_bot.MyDiscordClient, args);

				await rs.DoLaterReply();
				await rs.SendMessage(new UniversalMessageBuilder().SetContent("Good job!"));

				await Task.WhenAll(Enumerable.Range(1, 8).Select(x => DoTheThing(args)));
			}
			catch (Exception e)
			{
				Console.WriteLine($"{e}");
			}
		}
		private async Task DoTheThing(DiscordInteraction args)
		{
			var btn = new DiscordButtonComponent(ButtonStyle.Primary, "gfff", "fggg");
			var msg = new UniversalSession(new SessionFromMessage(_bot.MyDiscordClient,
				await args.Channel.SendMessageAsync(
				new UniversalMessageBuilder().SetContent("DDDD").AddComponents(btn)), args.User.Id));

			var intr = await msg.GetComponentInteraction();

			await msg.DoLaterReply();
			await msg.SendMessage(new UniversalMessageBuilder().SetContent("Good job!"));
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
			await args.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

			using var fdb = await _bot.DbFactory.CreateMyDbContextAsync();
			var db = fdb.ImplementedContext.Database;

			await db.EnsureDeletedAsync();
			await db.EnsureCreatedAsync();
		}

		private async Task CopyStructure()
		{

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
