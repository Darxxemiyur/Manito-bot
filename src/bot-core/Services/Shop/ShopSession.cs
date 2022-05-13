using System;
using System.Collections;
using System.Threading.Tasks;
using System.Linq;
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
using DSharpPlus.CommandsNext.Converters;
using Manito.Discord.FastJoin;
using System.Net;
using Manito.Discord.Economy;

namespace Manito.Discord.Shop
{

    public class ShopSession
    {
        private DiscordUser _customer;
        public DiscordUser Customer => _customer;
        private ShopCashRegister _cashRegister;
        private MyDiscordClient _client;
        private Action<ShopSession> _onExit;
        private ServerEconomy _economy;
        public ShopSession(MyDiscordClient client, DiscordUser customer, ServerEconomy economy,
         ShopCashRegister cashRegister, Action<ShopSession> onExit)
        {
            _client = client;
            _cashRegister = cashRegister;
            _customer = customer;
            _onExit = onExit;
            _economy = economy;
        }
        private DiscordEmbedBuilder BaseContent(DiscordEmbedBuilder bld = null) =>
            _cashRegister.Default(bld)
            .WithFooter($"{_args.User.Mention}", $"{_args.User.AvatarUrl}")
            .WithAuthor($"{_args.User.Username}#{_args.User.Discriminator}",
             null, $"{_args.User.AvatarUrl}");
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
        }

        private DiscordEmbedBuilder GetShopItems(DiscordEmbedBuilder prev = null)
        {
            var emb = prev ?? BaseContent();
            var str = _cashRegister.GetShopItems().Aggregate(emb, (x, y) =>
            {
                var price = $"{_economy.CurrencyEmoji} {y.Price}";
                return x.AddField($"**{y.Name}**", "**Цена за 1 ед:** " + price, true);
            });
            return emb;

        }
        private DiscordSelectComponent GetSelector(IEnumerable<ShopItem> list = null)
        {

            var items = (list ?? _cashRegister.GetShopItems())
            .Select(x => new DiscordSelectComponentOption(x.Name, x.Name, $"{x.Price}",
                false, new DiscordComponentEmoji(_economy.CurrencyEmojiId)));
            return new DiscordSelectComponent("Selection", "Выберите товар", items);

        }
        private bool _irtt;
        private InteractionResponseType GetIRT()
        {
            var irtt = _irtt;
            _irtt = true;
            return irtt ? InteractionResponseType.UpdateMessage
             : InteractionResponseType.ChannelMessageWithSource;
        }
        private Task Respond(DiscordInteractionResponseBuilder bld = default) =>
        _args.CreateResponseAsync(GetIRT(), bld?.AsEphemeral(false));

        private async Task ItemSelected(string itemId, ulong chId)
        {
            var items = _cashRegister.GetShopItems();

            var item = items.First(x => x.Name == itemId);

            var amt = await SelectingQuantity(item.Name, chId);

            await _economy.Withdraw(_args.User.Id, item.Price * amt);
        }

        private IEnumerable<DiscordButtonComponent> Generate(
            int[] nums, int mul) => nums.Select(x => new DiscordButtonComponent(ButtonStyle.Secondary,
             $"{(x > 0 ? "add" : "sub")}{Math.Abs(x * mul)}", (x > 0 ? "+" : "") + $"{x * mul}"));
        private IEnumerable<IEnumerable<DiscordButtonComponent>> Generate(
         int[] nums, int[] muls) => muls.Select(x => Generate(nums, x));
        private async Task<int> SelectingQuantity(string itemName, ulong chId)
        {
            var amount = 0;

            var btns = Generate(new[] { -5, 1, 2, 5 }, new[] { 1, 10, 100 });

            while (true)
            {
                var ms1 = $"Выберите количество {itemName}";
                var ms2 = $"Выбранное количество {amount} ед.";
                var mg2 = GetResponse(BaseContent().WithDescription($"{ms1}\n{ms2}"));

                foreach (var row in btns)
                    mg2.AddComponents(row);


                mg2.AddComponents(
                    new DiscordButtonComponent(ButtonStyle.Danger, "Exit", "Назад"),
                    new DiscordButtonComponent(ButtonStyle.Success, "Submit", "Выбрать"));


                await Respond(mg2);


                _args = (await _client.ActivityTools.WaitForComponentInteraction(x =>
                    x.Message.ChannelId == chId && x.User.Id == _args.User.Id &&
                    mg2.Components.SelectMany(y => y.Components)
                    .Any(y => x.Interaction.Data.CustomId == y.CustomId))).Interaction;

                if (_args.Data.CustomId == "Exit")
                {
                    amount = 0;
                    break;
                }

                if (_args.Data.CustomId == "Submit")
                    break;

                var pressed = btns.SelectMany(x => x).First(x => x.CustomId == _args.Data.CustomId);

                var change = int.Parse(pressed.Label);

                amount = Math.Clamp(amount + change, 0, int.MaxValue);
            }

            return amount;
        }

        private DiscordInteraction _args;
        public async Task EnterMenu(DiscordInteraction args, ulong chId)
        {
            _args = args;
            try
            {
                var items = GetSelector();
                var exbtn = new DiscordButtonComponent(ButtonStyle.Danger, "Exit", "Выйти");
                while (true)
                {
                    var mg = GetResponse(GetShopItems()).AddComponents(items).AddComponents(exbtn);
                    await Respond(mg);

                    var argv = await _client.ActivityTools.WaitForComponentInteraction(x =>
                        x.Message.ChannelId == chId && x.User.Id == args.User.Id &&
                        mg.Components.SelectMany(x => x.Components)
                        .Any(y => x.Interaction.Data.CustomId == y.CustomId));

                    _args = argv.Interaction;

                    if (_args.Data.CustomId == exbtn.CustomId)
                        break;

                    await ItemSelected(argv.Values[0], chId);

                }

                await Respond(GetResponse(BaseContent().WithDescription("Сессия успешно завершена.")));
                StopSession();

                await Task.Delay(10000);
                await _args.DeleteOriginalResponseAsync();
            }
            catch (TimeoutException)
            {
                var ms = "Сессия завершена по причине привешения времени ожидания взаимодействия.";
                var gld = _client.Client;
                var chnl = await gld.GetChannelAsync(chId);
                await _args.DeleteOriginalResponseAsync();
                var msgtd = await chnl.SendMessageAsync(GetDResponse(BaseContent().WithDescription(ms)));
                StopSession();
                await msgtd.DeleteAsync();
            }
        }
    }

}
