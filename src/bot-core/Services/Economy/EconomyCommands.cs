using System;
using System.Collections.Generic;
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
        private IEconomyDbFactory _dbFactory;
        public EconomyCommands()
        {

        }
        public async Task HandleCommands(DiscordInteraction command)
        {
            foreach (var item in GetCommands())
            {
                if (command.Data.Name.Contains(item.Item1.Name))
                    await item.Item2(command);
            }
        }
        public IEnumerable<(DiscordApplicationCommand, Func<DiscordInteraction, Task>)> GetCommands()
        {
            yield return (new DiscordApplicationCommand("account", "показать счёт",
            defaultPermission: true), GetAccountDeposit);
        }
        /// <summary>
        /// Get user's Account deposit.
        /// </summary>
        /// <param name="user">Target user. Itself if not specified.</param>
        /// <returns></returns>
        private async Task GetAccountDeposit(DiscordInteraction args)
        {
            var deposit = new PlayerEconomyDeposit();


            var msg = new DiscordInteractionResponseBuilder(new DiscordMessageBuilder()
            .WithEmbed(new DiscordEmbedBuilder().WithDescription(deposit.Currency + " чешуек")
            .WithTitle("Валюта").WithAuthor(args.User.Username)));

            await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
        }

        public async Task TransferMoney(DiscordUser user)
        {

        }
    }

}
