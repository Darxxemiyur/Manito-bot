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

    public class EconomyCommands
    {
        private const string Locale = "ru";
        private ServerEconomy _economy;
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
        private Dictionary<string, string> GetLoc(string trans) => new Dictionary<string, string>() { { Locale, trans } };
        private IEnumerable<(DiscordApplicationCommandOption, Func<DiscordInteraction, Task>)> GetSubCommands()
        {
            yield return (new DiscordApplicationCommandOption("account", "Show currency",
             ApplicationCommandOptionType.SubCommand, null, null, new[] {
                new DiscordApplicationCommandOption("target", "Account", ApplicationCommandOptionType.User,
                 false, name_localizations: GetLoc( "счёт"),
                 description_localizations: GetLoc( "Счёт"))
             },
             name_localizations: GetLoc("посмотреть"),
             description_localizations: GetLoc("Посмотреть средства")),
             GetAccountDeposit);

            yield return (new DiscordApplicationCommandOption("transfer", "Transfer funds",
             ApplicationCommandOptionType.SubCommand, null, null, new[] {
                new DiscordApplicationCommandOption("target", "Recipient", ApplicationCommandOptionType.User,
                 true, name_localizations: GetLoc("получатель"),
                 description_localizations: GetLoc("Получатель")),
                new DiscordApplicationCommandOption("amount", "Amount", ApplicationCommandOptionType.Integer,
                 true, name_localizations: GetLoc("сумма"),
                 description_localizations: GetLoc("Сумма"))
             },
             name_localizations: GetLoc("перевести"),
             description_localizations: GetLoc("Перевести средства")),
             TransferMoney);

            yield return (new DiscordApplicationCommandOption("give", "Add funds",
             ApplicationCommandOptionType.SubCommand, null, null, new[] {
                new DiscordApplicationCommandOption("target", "Account", ApplicationCommandOptionType.User,
                 true, name_localizations: GetLoc("счёт"),
                 description_localizations: GetLoc("Счёт")),
                new DiscordApplicationCommandOption("amount", "Amount", ApplicationCommandOptionType.Integer,
                 true, name_localizations: GetLoc("сумма"),
                 description_localizations:  GetLoc("Сумма"))
             },
             name_localizations: GetLoc("добавить"),
             description_localizations: GetLoc("Добавить средства")),
             Deposit);

            yield return (new DiscordApplicationCommandOption("take", "Remove funds",
             ApplicationCommandOptionType.SubCommand, null, null, new[] {
                new DiscordApplicationCommandOption("target", "Account", ApplicationCommandOptionType.User,
                 true, name_localizations: GetLoc("счёт"),
                 description_localizations: GetLoc("Счёт")),
                new DiscordApplicationCommandOption("amount", "Amount", ApplicationCommandOptionType.Integer,
                 true, name_localizations:GetLoc("сумма"),
                 description_localizations: GetLoc("Сумма"))
             },
             name_localizations: GetLoc("удалить"),
             description_localizations: GetLoc("Удалить средства")),
             Withdraw);
        }
        public IEnumerable<DiscordApplicationCommand> GetCommands()
        {
            yield return new DiscordApplicationCommand("bank", "Bank",
             GetSubCommands().Select(x => x.Item1), true,
             ApplicationCommandType.SlashCommand,
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
            var argtools = new AppArgsTools(args);

            var tgt = argtools.AddReqArg("target");
            var amot = argtools.AddReqArg("amount");

            var msg = new DiscordInteractionResponseBuilder();

            if (!argtools.DoHaveReqArgs())
            {
                msg.WithContent($"Неправильно введены аргументы!");
                await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
                return;
            }

            var from = args.User.Id;
            var to = (ulong)GetItem(argtools, tgt);
            var amt = (long)GetItem(argtools, amot);

            amt = await _economy.TransferFunds(from, to, amt);

            msg.WithContent($"Успешно переведено {amt} {_economy.CurrencyEmoji} на счёт <@{to}>");

            await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
        }
        private async Task Withdraw(DiscordInteraction args)
        {
            var argtools = new AppArgsTools(args);

            var tgt = argtools.AddReqArg("target");
            var amot = argtools.AddReqArg("amount");

            var msg = new DiscordInteractionResponseBuilder();

            if (!argtools.DoHaveReqArgs())
            {
                msg.WithContent($"Неправильно введены аргументы!");
                await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
                return;
            }

            var to = (ulong)GetItem(argtools, tgt);
            var amt = (long)GetItem(argtools, amot);

            amt = await _economy.Withdraw(to, amt);

            msg.WithContent($"Успешно удалено {amt} {_economy.CurrencyEmoji} со счёта <@{to}>");

            await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
        }
        private Object GetItem(AppArgsTools args, string value) =>
            args.GetReq().FirstOrDefault(x => x.Key == value).Value;

        private async Task Deposit(DiscordInteraction args)
        {
            var argtools = new AppArgsTools(args);

            var tgt = argtools.AddReqArg("target");
            var amot = argtools.AddReqArg("amount");

            var msg = new DiscordInteractionResponseBuilder();

            if (!argtools.DoHaveReqArgs())
            {
                msg.WithContent($"Неправильно введены аргументы!");
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
