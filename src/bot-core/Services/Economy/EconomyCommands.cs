using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;


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
        private IEnumerable<(DiscordApplicationCommandOption, Func<DiscordInteraction, Task>)> GetSubCommands()
        {
            yield return (new DiscordApplicationCommandOption("account", "Show currency",
             ApplicationCommandOptionType.SubCommand, null, null, new[] {
                new DiscordApplicationCommandOption("target", "Account", ApplicationCommandOptionType.User,
                 false, name_localizations: new Dictionary<string, string>() { { Locale, "счёт" } },
                 description_localizations: new Dictionary<string, string>() { { Locale, "Счёт" } })
             },
             name_localizations: new Dictionary<string, string>() { { Locale, "посмотреть" } },
             description_localizations: new Dictionary<string, string>() { { Locale, "Посмотреть средства" } }),
             GetAccountDeposit);

            yield return (new DiscordApplicationCommandOption("transfer", "Transfer funds",
             ApplicationCommandOptionType.SubCommand, null, null, new[] {
                new DiscordApplicationCommandOption("target", "Recipient", ApplicationCommandOptionType.User,
                 true, name_localizations: new Dictionary<string, string>() { { Locale, "получатель" }},
                 description_localizations: new Dictionary<string, string>() { { Locale, "Получатель" } }),
                new DiscordApplicationCommandOption("amount", "Amount", ApplicationCommandOptionType.Integer,
                 true, name_localizations: new Dictionary<string, string>() { { Locale, "сумма" } },
                 description_localizations: new Dictionary<string, string>() { { Locale, "Сумма" } })
             },
             name_localizations: new Dictionary<string, string>() { { Locale, "перевести" } },
             description_localizations: new Dictionary<string, string>() { { Locale, "Перевести средства" } }),
             TransferMoney);

            yield return (new DiscordApplicationCommandOption("give", "Add funds",
             ApplicationCommandOptionType.SubCommand, null, null, new[] {
                new DiscordApplicationCommandOption("target", "Account", ApplicationCommandOptionType.User,
                 true, name_localizations: new Dictionary<string, string>() { { Locale, "счёт" } },
                 description_localizations: new Dictionary<string, string>() { { Locale, "Счёт" } }),
                new DiscordApplicationCommandOption("amount", "Amount", ApplicationCommandOptionType.Integer,
                 true, name_localizations: new Dictionary<string, string>() { { Locale, "сумма" } },
                 description_localizations: new Dictionary<string, string>() { { Locale, "Сумма" } })
             },
             name_localizations: new Dictionary<string, string>() { { Locale, "добавить" } },
             description_localizations: new Dictionary<string, string>() { { Locale, "Добавить средства" } }),
             Deposit);
             
            yield return (new DiscordApplicationCommandOption("take", "Remove funds",
             ApplicationCommandOptionType.SubCommand, null, null, new[] {
                new DiscordApplicationCommandOption("target", "Account", ApplicationCommandOptionType.User,
                 true, name_localizations: new Dictionary<string, string>() { { Locale, "счёт" } },
                 description_localizations: new Dictionary<string, string>() { { Locale, "Счёт" } }),
                new DiscordApplicationCommandOption("amount", "Amount", ApplicationCommandOptionType.Integer,
                 true, name_localizations: new Dictionary<string, string>() { { Locale, "сумма" } },
                 description_localizations: new Dictionary<string, string>() { { Locale, "Сумма" } })
             },
             name_localizations: new Dictionary<string, string>() { { Locale, "удалить" } },
             description_localizations: new Dictionary<string, string>() { { Locale, "Удалить средства" } }),
             Withdraw);
        }
        public IEnumerable<DiscordApplicationCommand> GetCommands()
        {
            yield return new DiscordApplicationCommand("bank", "Bank",
             GetSubCommands().Select(x => x.Item1), true,
             ApplicationCommandType.SlashCommand,
             new Dictionary<string, string>() { { Locale, "banj" } },
             new Dictionary<string, string>() { { Locale, "Banj" } });

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

        public async Task TransferMoney(DiscordInteraction args)
        {
            var from = args.User.Id;
            var to = (ulong)args.Data.Options.First().Options.First(x => x.Name == "target").Value;
            var amt = (long)args.Data.Options.First().Options.First(x => x.Name == "amount").Value;

            amt = await _economy.TransferFunds(from, to, amt);

            var msg = new DiscordInteractionResponseBuilder();
            msg.WithContent($"Успешно переведено {amt} {_economy.CurrencyEmoji} на счёт <@{to}>");

            await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
        }
        public async Task Withdraw(DiscordInteraction args)
        {
            var to = (ulong)args.Data.Options.First().Options.First(x => x.Name == "target").Value;
            var amt = (long)args.Data.Options.First().Options.First(x => x.Name == "amount").Value;

            await _economy.Withdraw(to, amt);

            var msg = new DiscordInteractionResponseBuilder();
            msg.WithContent($"Успешно удалено {amt} {_economy.CurrencyEmoji} со счёта <@{to}>");

            await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
        }
        public async Task Deposit(DiscordInteraction args)
        {
            var to = (ulong)args.Data.Options.First().Options.First(x => x.Name == "target").Value;
            var amt = (long)args.Data.Options.First().Options.First(x => x.Name == "amount").Value;

            await _economy.Deposit(to, amt);

            var msg = new DiscordInteractionResponseBuilder();
            msg.WithContent($"Успешно добавлено {amt} {_economy.CurrencyEmoji} на счёт <@{to}>");

            await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
        }
    }

}
