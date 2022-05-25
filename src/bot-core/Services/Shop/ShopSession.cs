using System;
using System.Threading.Tasks;
using System.Linq;

using DSharpPlus;
using DSharpPlus.Entities;

using Manito.Discord.Client;
using System.Collections.Generic;
using Manito.Discord.Economy;

namespace Manito.Discord.Shop
{

    public class ShopSession
    {
        private DiscordUser _customer;
        private ShopCashRegister _cashRegister;
        private MyDiscordClient _client;
        private Action<ShopSession> _onExit;
        private ServerEconomy _economy;
        private DiscordInteraction _args;
        public DiscordUser Customer => _customer;
        public ShopCashRegister CashRegister => _cashRegister;
        public MyDiscordClient Client => _client;
        public ServerEconomy Economy => _economy;
        public DiscordInteraction Args
        {
            get => _args;
            set => _args = value;
        }

        public ShopSession(MyDiscordClient client, DiscordUser customer, ServerEconomy economy,
         ShopCashRegister cashRegister, Action<ShopSession> onExit)
        {
            _client = client;
            _cashRegister = cashRegister;
            _customer = customer;
            _onExit = onExit;
            _economy = economy;
        }
        public DiscordEmbedBuilder BaseContent(DiscordEmbedBuilder bld = null) =>
            _cashRegister.Default(bld)
            .WithFooter($"{_args.User.Mention}", $"{_args.User.AvatarUrl}")
            .WithAuthor($"{_args.User.Username}#{_args.User.Discriminator}",
             null, $"{_args.User.AvatarUrl}");
        public DiscordMessageBuilder GetDResponse(DiscordEmbedBuilder builder = null)
        {

            return new DiscordMessageBuilder().WithEmbed(builder ?? BaseContent());

        }
        public DiscordInteractionResponseBuilder GetResponse(DiscordEmbedBuilder builder = null)
        {

            return new DiscordInteractionResponseBuilder(GetDResponse(builder));

        }
        private void StopSession()
        {
            _onExit(this);
        }

        private DiscordEmbedBuilder GetShopItems(DiscordEmbedBuilder prev = null,
         IEnumerable<ShopItem> list = null)
        {
            var emb = prev ?? BaseContent();
            var str = (list ?? _cashRegister.GetShopItems()).Aggregate(emb, (x, y) =>
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
        public Task Respond(DiscordInteractionResponseBuilder bld = default) =>
        _args.CreateResponseAsync(GetIRT(), bld?.AsEphemeral(false));

        private IBuyingSteps CallChain(ShopItem item)
        {
            switch (item.Category)
            {
                case ShopItemCategory.SatiationCarcass:
                case ShopItemCategory.Carcass:
                case ShopItemCategory.Plant:
                    return new BuyingStepsForFood(this, item);

                default:
                    throw new NotImplementedException();
            }
        }
        private async Task ItemSelected(ShopItem item, ulong chId)
        {
            var chain = CallChain(item);
            await BuyingStepsCommon.RunChain(chain, x => Task.FromResult(x.NextAction != NextActions.Continue), chId);
        }

        public async Task EnterMenu(DiscordInteraction args, ulong chId)
        {
            _args = args;
            try
            {
                var exbtn = new DiscordButtonComponent(ButtonStyle.Danger, "Exit", "Выйти");
                while (true)
                {
                    var shopItems = _cashRegister.GetShopItems();
                    var items = GetSelector(shopItems);
                    var mg = GetResponse(GetShopItems(null, shopItems)).AddComponents(items)
                    .AddComponents(exbtn);
                    await Respond(mg);

                    var argv = await _client.ActivityTools.WaitForComponentInteraction(x =>
                        x.Message.ChannelId == chId && x.User.Id == args.User.Id &&
                        mg.Components.SelectMany(x => x.Components)
                        .Any(y => x.Interaction.Data.CustomId == y.CustomId));

                    _args = argv.Interaction;

                    if (_args.Data.CustomId == exbtn.CustomId)
                        break;

                    await ItemSelected(shopItems.First(x => x.Name == argv.Values[0]), chId);

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
