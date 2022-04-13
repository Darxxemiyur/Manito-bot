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

namespace Manito.Discord.Shop
{

    public class ShopSession
    {
        private DiscordUser _customer;
        public DiscordUser Customer => _customer;
        private ShopCashRegister _cashRegister;
        private MyDiscordClient _client;
        public ShopSession(MyDiscordClient client, DiscordUser customer, ShopCashRegister cashRegister)
        {
            _client = client;
            _cashRegister = cashRegister;
            _customer = customer;
        }
        private DiscordEmbedBuilder BaseContent() =>
        new DiscordEmbedBuilder().WithTitle("~Магазин Манито~");
        private DiscordMessageBuilder GetDResponse(DiscordEmbedBuilder builder = null)
        {

            return new DiscordMessageBuilder().WithEmbed(builder ?? BaseContent());

        }
        private DiscordInteractionResponseBuilder GetResponse(DiscordEmbedBuilder builder = null)
        {

            return new DiscordInteractionResponseBuilder(GetDResponse(builder));

        }
        private DiscordEmbedBuilder GetShopItems(DiscordEmbedBuilder prev = null)
        {
            var emb = prev ?? new DiscordEmbedBuilder();

            var items = _cashRegister.GetShopItems();

            var str = items.Aggregate<ShopItem, String>("", (x, y) =>
            {
                var price = y.Price;
                return x += "\n\n**Название:** " + y.Name + "\n**Категория:** "
                + y.Category.ToString() + "\n**Цена за единицу:** " + price.ToString();
            });


            return emb.WithDescription(emb.Description + str);

        }
        public async Task EnterMenu(DiscordInteraction args)
        {
            await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
             GetResponse(GetShopItems()));
            var origMsg = await args.GetOriginalResponseAsync();


            while (true)
            {
                var response = await _client.ActivityTools.WaitForMessage((x) =>
                    x.Channel.Id == args.Channel.Id && x.Author.Id == args.User.Id);
            }

        }
    }

}
