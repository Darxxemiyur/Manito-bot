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

using Manito.Discord.Client;
using Name.Bayfaderix.Darxxemiyur.Common;
using Manito.Discord.ChatNew;
using static System.Collections.Specialized.BitVector32;

namespace Manito.Discord.Economy
{

	public class EconomyCommands
	{
		private const string Locale = "ru";
		private readonly ServerEconomy _economy;
		private readonly MyDiscordClient _client;
		public EconomyCommands(ServerEconomy economy, MyDiscordClient client) => (_economy, _client) = (economy, client);
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
		private DiscordApplicationCommandLocalization GetLoc(string trans) => new(new() { { Locale, trans } });
		private IEnumerable<(DiscordApplicationCommandOption, Func<DiscordInteraction, Task>)> GetSubCommands()
		{
			yield return (new DiscordApplicationCommandOption("account", "Show currency",
			 ApplicationCommandOptionType.SubCommand, false, null, new[] {
				new DiscordApplicationCommandOption("target", "Account", ApplicationCommandOptionType.User,
				 false, nameLocalizations: GetLoc( "счёт"),
				 descriptionLocalizations: GetLoc( "Счёт"))
			 },
			 nameLocalizations: GetLoc("посмотреть"),
			 descriptionLocalizations: GetLoc("Посмотреть средства")),
			 GetAccountDeposit);

			yield return (new DiscordApplicationCommandOption("transfer", "Transfer funds",
			 ApplicationCommandOptionType.SubCommand, false, null, new[] {
				new DiscordApplicationCommandOption("target", "Recipient", ApplicationCommandOptionType.User,
				 true, nameLocalizations: GetLoc("получатель"),
				 descriptionLocalizations: GetLoc("Получатель")),
				new DiscordApplicationCommandOption("amount", "Amount", ApplicationCommandOptionType.Integer,
				 true, nameLocalizations: GetLoc("сумма"),
				 descriptionLocalizations: GetLoc("Сумма"))
			 },
			 nameLocalizations: GetLoc("перевести"),
			 descriptionLocalizations: GetLoc("Перевести средства")),
			 TransferMoney);

			yield return (new DiscordApplicationCommandOption("give", "Add funds",
			 ApplicationCommandOptionType.SubCommand, false, null, new[] {
				new DiscordApplicationCommandOption("target", "Account", ApplicationCommandOptionType.User,
				 true, nameLocalizations: GetLoc("счёт"),
				 descriptionLocalizations: GetLoc("Счёт")),
				new DiscordApplicationCommandOption("amount", "Amount", ApplicationCommandOptionType.Integer,
				 true, nameLocalizations: GetLoc("сумма"),
				 descriptionLocalizations:  GetLoc("Сумма"))
			 },
			 nameLocalizations: GetLoc("добавить"),
			 descriptionLocalizations: GetLoc("Добавить средства")),
			 Deposit);

			yield return (new DiscordApplicationCommandOption("take", "Remove funds",
			 ApplicationCommandOptionType.SubCommand, false, null, new[] {
				new DiscordApplicationCommandOption("target", "Account", ApplicationCommandOptionType.User,
				 true, nameLocalizations: GetLoc("счёт"),
				 descriptionLocalizations: GetLoc("Счёт")),
				new DiscordApplicationCommandOption("amount", "Amount", ApplicationCommandOptionType.Integer,
				 true, nameLocalizations:GetLoc("сумма"),
				 descriptionLocalizations: GetLoc("Сумма"))
			 },
			 nameLocalizations: GetLoc("удалить"),
			 descriptionLocalizations: GetLoc("Удалить средства")),
			 Withdraw);
		}
		public IEnumerable<DiscordApplicationCommand> GetCommands()
		{
			yield return new DiscordApplicationCommand("bank", "Bank",
			 GetSubCommands().Select(x => x.Item1),
			 ApplicationCommandType.ChatInput,
			 GetLoc("банк"),
			 GetLoc("Банк"));

		}
		/// <summary>
		/// Get user's Account deposit.
		/// </summary>
		/// <returns></returns>
		private async Task GetAccountDeposit(DiscordInteraction args)
		{
			var target = args.User.Id;

			var oth = args.Data.Options.First().Options.FirstOrDefault(x => x.Name == "target");
			if (oth != null)
				target = (ulong)oth.Value;

			var session = new ComponentDialogueSession(_client, args);
			await session.DoLaterReply();

			var deposit = await _economy.GetPlayerWallet(target).GetScales();


			var msg = new UniversalMessageBuilder()
			 .AddEmbed(new DiscordEmbedBuilder()
			 .WithDescription(deposit + $" {_economy.CurrencyEmoji}")
			 .WithTitle("Валюта").WithAuthor($"<@{target}>"));

			await session.SendMessage(msg);
		}

		private async Task TransferMoney(DiscordInteraction args)
		{
			var argtools = new AppCommandArgsTools(args);

			var tgt = argtools.AddReqArg("target");
			var amot = argtools.AddReqArg("amount");
			var session = new ComponentDialogueSession(_client, args);

			var msg = new DiscordInteractionResponseBuilder();

			if (!argtools.DoHaveReqArgs())
			{
				msg.WithContent(args.Locale == "ru"
				 ? "Неправильно введены аргументы!" : "Wrong arguments!");
				await session.SendMessage(msg);
				return;
			}
			await session.DoLaterReply();

			var from = args.User.Id;
			var to = argtools.GetArg<ulong>(tgt);
			var amt = (long)argtools.GetArg<int>(amot);

			amt = await _economy.TransferFunds(from, to, amt);

			msg.WithContent(args.Locale == "ru"
				 ? $"Успешно переведено {amt} {_economy.CurrencyEmoji} на счёт <@{to}>"
				 : $"Succesfuly transfered {amt} {_economy.CurrencyEmoji} to <@{to}>'s account");

			await session.SendMessage(msg);
		}
		private async Task Withdraw(DiscordInteraction args)
		{
			var argtools = new AppCommandArgsTools(args);

			var tgt = argtools.AddReqArg("target");
			var amot = argtools.AddReqArg("amount");

			var session = new ComponentDialogueSession(_client, args);
			var msg = new DiscordInteractionResponseBuilder();

			if (!argtools.DoHaveReqArgs())
			{
				msg.WithContent(args.Locale == "ru"
				 ? "Неправильно введены аргументы!" : "Wrong arguments!");
				await session.SendMessage(msg);
				return;
			}

			await session.DoLaterReply();

			var to = argtools.GetArg<ulong>(tgt);
			var amt = (long)argtools.GetArg<int>(amot);

			amt = await _economy.Withdraw(to, amt);

			msg.WithContent($"Успешно удалено {amt} {_economy.CurrencyEmoji} со счёта <@{to}>");

			await session.SendMessage(msg);
		}
		private async Task Deposit(DiscordInteraction args)
		{
			var argtools = new AppCommandArgsTools(args);

			var tgt = argtools.AddReqArg("target");
			var amot = argtools.AddReqArg("amount");

			var msg = new DiscordInteractionResponseBuilder();
			var session = new ComponentDialogueSession(_client, args);

			if (!argtools.DoHaveReqArgs())
			{
				msg.WithContent(args.Locale == "ru"
				 ? "Неправильно введены аргументы!" : "Wrong arguments!");
				await session.SendMessage(msg);
				return;
			}

			await session.DoLaterReply();

			var to = argtools.GetArg<ulong>(tgt);
			var amt = (long)argtools.GetArg<int>(amot);

			amt = await _economy.Deposit(to, amt);
			msg.WithContent($"Успешно добавлено {amt} {_economy.CurrencyEmoji} на счёт <@{to}>");

			await session.SendMessage(msg);

		}
	}

}
