using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Manito.Discord.ChatNew;
using Manito.Discord.Client;

using Microsoft.EntityFrameworkCore;

using Name.Bayfaderix.Darxxemiyur.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Manito.System.Economy
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

		private Task<bool> IsWorthy(DiscordUser user) => _client.Domain.Filters.AssociationFilter.PermissionChecker.DoesHaveAdminPermission(this, user);

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

			yield return (new DiscordApplicationCommandOption("work", "Do work",
			 ApplicationCommandOptionType.SubCommand, false, null,// new[] {
																  //new DiscordApplicationCommandOption("target", "Account", ApplicationCommandOptionType.User,  false, nameLocalizations: GetLoc( "счёт"), descriptionLocalizations: GetLoc( "Счёт"))
																  //},
			 nameLocalizations: GetLoc("заработать"),
			 descriptionLocalizations: GetLoc("Заработать средства")),
			 DoWork);

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
			 .WithDescription($"{deposit} {_economy.CurrencyEmoji} у <@{target}>")
			 .WithTitle("Валюта"));

			await session.SendMessage(msg);
		}

		/// <summary>
		/// Get user's Account deposit.
		/// </summary>
		/// <returns></returns>
		private async Task DoWork(DiscordInteraction args)
		{
			var target = args.User.Id;

			//var oth = args.Data.Options.First().Options.FirstOrDefault(x => x.Name == "target");
			//if (oth != null)
			//	target = (ulong)oth.Value;
			var session = new ComponentDialogueSession(_client, args);
			var wallet = _economy.GetPlayerWallet(target);
			await session.DoLaterReply();
#if DEBUG
			var time = TimeSpan.FromSeconds(20);
#else
			var time = TimeSpan.FromHours(4);
#endif
			await using var _ = await _lock.BlockAsyncLock();
			await using var db = await _client.Domain.DbFactory.CreateMyDbContextAsync();

			if (!await db.PlayerWorks.AnyAsync(x => x.DiscordID == target))
			{
				await db.PlayerWorks.AddAsync(new PlayerEconomyWork(target));
				await db.SaveChangesAsync();
			}

			var work = await db.PlayerWorks.FirstAsync(x => x.DiscordID == target);
			if (DateTimeOffset.UtcNow - work.LastWork < time)
			{
				var delay = DateTimeOffset.UtcNow - work.LastWork;
				var small = TimeSpan.FromSeconds(10);

				delay = time - delay > small ? small : time - delay;
				await _client.Domain.ExecutionThread.AddNew(async () => {
					await session.SendMessage(new DiscordEmbedBuilder().WithDescription($"Вы уже работали!\nВы сможете работать <t:{(work.LastWork + time).ToUnixTimeSeconds()}:R>").WithColor(new DiscordColor(240, 140, 50)));
					await Task.Delay(delay);
					await session.RemoveMessage();
				});
				return;
			}
			else
			{
				work.LastWork = DateTimeOffset.UtcNow;
				work.TimesWorked++;
			}
			await db.SaveChangesAsync();

			await _client.Domain.ExecutionThread.AddNew(async () => {
				var money = Random.Shared.Next(270, 500);
				await session.SendMessage(new DiscordEmbedBuilder().WithDescription($"Заработано {money} {wallet.CurrencyEmoji}").WithColor(new DiscordColor(140, 240, 50)));

				await wallet.Deposit(money, "Заработок");
				await Task.Delay(10000);
				await session.RemoveMessage();
			});
		}

		private AsyncLocker _lock = new();

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
			if (!await IsWorthy(args.User))
			{
				var msgnw = new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder().WithDescription("Недостаточно прав!")).AsEphemeral();
				await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msgnw);
				return;
			}

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
			if (!await IsWorthy(args.User))
			{
				var msgnw = new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder().WithDescription("Недостаточно прав!")).AsEphemeral();
				await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msgnw);
				return;
			}

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