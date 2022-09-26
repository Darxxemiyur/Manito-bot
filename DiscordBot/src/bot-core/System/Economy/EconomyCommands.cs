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

namespace Manito.Discord.Economy
{

	public class EconomyCommands
	{
		private const string Locale = "ru";
		private readonly ServerEconomy _economy;
		public EconomyCommands(ServerEconomy economy) => _economy = economy;
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

			var deposit = _economy.GetDeposit(target);


			var msg = new DiscordInteractionResponseBuilder(new DiscordMessageBuilder()
			 .WithEmbed(new DiscordEmbedBuilder()
			 .WithDescription(deposit.Currency + $" {_economy.CurrencyEmoji}")
			 .WithTitle("Валюта").WithAuthor($"<@{target}>")));

			await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);

		}

		private async Task TransferMoney(DiscordInteraction args)
		{
			var argtools = new AppCommandArgsTools(args);

			var tgt = argtools.AddReqArg("target");
			var amot = argtools.AddReqArg("amount");

			var msg = new DiscordInteractionResponseBuilder();

			if (!argtools.DoHaveReqArgs())
			{
				msg.WithContent(args.Locale == "ru"
				 ? "Неправильно введены аргументы!" : "Wrong arguments!");
				await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
				return;
			}

			var from = args.User.Id;
			var to = (ulong)GetItem(argtools, tgt);
			var amt = (long)GetItem(argtools, amot);

			amt = await _economy.TransferFunds(from, to, amt);

			msg.WithContent(args.Locale == "ru"
				 ? $"Успешно переведено {amt} {_economy.CurrencyEmoji} на счёт <@{to}>"
				 : $"Succesfuly transfered {amt} {_economy.CurrencyEmoji} to <@{to}>'s account");

			await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
		}
		private async Task Withdraw(DiscordInteraction args)
		{
			var argtools = new AppCommandArgsTools(args);

			var tgt = argtools.AddReqArg("target");
			var amot = argtools.AddReqArg("amount");

			var msg = new DiscordInteractionResponseBuilder();

			if (!argtools.DoHaveReqArgs())
			{
				msg.WithContent(args.Locale == "ru"
				 ? "Неправильно введены аргументы!" : "Wrong arguments!");
				await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
				return;
			}

			var to = (ulong)GetItem(argtools, tgt);
			var amt = (long)GetItem(argtools, amot);

			amt = await _economy.Withdraw(to, amt);

			msg.WithContent($"Успешно удалено {amt} {_economy.CurrencyEmoji} со счёта <@{to}>");

			await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
		}
		private Object GetItem(AppCommandArgsTools args, string value) =>
			args.GetReq().FirstOrDefault(x => x.Key == value).Value;

		private async Task Deposit(DiscordInteraction args)
		{
			var argtools = new AppCommandArgsTools(args);

			var tgt = argtools.AddReqArg("target");
			var amot = argtools.AddReqArg("amount");

			var msg = new DiscordInteractionResponseBuilder();

			if (!argtools.DoHaveReqArgs())
			{
				msg.WithContent(args.Locale == "ru"
				 ? "Неправильно введены аргументы!" : "Wrong arguments!");
				await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
				return;
			}

			var to = (ulong)GetItem(argtools, tgt);
			var amt = (long)GetItem(argtools, amot);

			amt = await _economy.Deposit(to, amt);
			msg.WithContent($"Успешно добавлено {amt} {_economy.CurrencyEmoji} на счёт <@{to}>");

			await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
		}
	}

}
