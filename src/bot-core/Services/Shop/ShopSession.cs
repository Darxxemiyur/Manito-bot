using System;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.EventArgs;

using Cyriller;

using Manito.Discord.Client;
using System.Collections.Generic;

namespace Manito.Discord.Shop
{

    public class ShopSession
    {
        private DiscordUser _customer;
        public DiscordUser Customer => _customer;
        private ShopCashRegister _cashRegister;
        private MyDiscordClient _client;
        private Action<ShopSession> _onExit;
        public ShopSession(MyDiscordClient client, DiscordUser customer,
         ShopCashRegister cashRegister, Action<ShopSession> onExit)
        {
            _client = client;
            _cashRegister = cashRegister;
            _customer = customer;
            _onExit = onExit;
        }
        private DiscordEmbedBuilder BaseContent() =>
        new DiscordEmbedBuilder().WithTitle("~Магазин Манито~").WithColor(DiscordColor.Blurple);
        private DiscordMessageBuilder GetDResponse(DiscordEmbedBuilder builder = null)
        {

            return new DiscordMessageBuilder().WithEmbed(builder ?? BaseContent());

        }
        private DiscordInteractionResponseBuilder GetResponse(DiscordEmbedBuilder builder = null)
        {

            return new DiscordInteractionResponseBuilder(GetDResponse(builder));

        }
        private void StopSession()
        {
            _onExit(this);
            throw new Exception();
        }

        private DiscordEmbedBuilder GetShopItems(DiscordEmbedBuilder prev = null)
        {
            var emb = prev ?? BaseContent();

            var items = _cashRegister.GetShopItems();
            var emj = "<:964951871435468810:964951871435468810>";

            var str = items.Aggregate(emb, (x, y) =>
            {
                var price = emj + " " + y.Price.ToString();
                return x.AddField(y.Name, "**Цена за 1 ед:** " + price, true);
            });


            return emb;

        }
        private async Task HandleMenu(DiscordInteraction args)
        {

            await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
             GetResponse(GetShopItems()));
            var origMsg = await args.GetOriginalResponseAsync();


            while (true)
            {
                var response = await _client.ActivityTools.WaitForMessage((x) =>
                    x.Channel.Id == args.Channel.Id && x.Author.Id == args.User.Id);

                if (response.Message.Content.Contains("exit", StringComparison.OrdinalIgnoreCase))
                    StopSession();

                await response.Message.RespondAsync("Купи дарагой да");
            }
        }
        public async Task EnterMenu(DiscordInteraction args)
        {
            try
            {
                await HandleMenu(args);

            }
            catch (Exception e)
            {
                return;
            }
        }
    }

}
