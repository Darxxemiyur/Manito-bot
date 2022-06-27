using System;
using System.Threading.Tasks;
using System.Linq;

using DSharpPlus;
using DSharpPlus.Entities;

using Manito.Discord.Client;
using System.Collections.Generic;
using Manito.Discord.Economy;
using DSharpPlus.EventArgs;
using Manito.Discord.Chat.DialogueNet;
using Manito.Discord.Inventory;

namespace Manito.Discord.Shop
{

    public class ShopSession
    {
        private DiscordUser _customer;
        private ShopCashRegister _cashRegister;
        private MyDiscordClient _client;
        private Action<ShopSession> _onExit;
        private PlayerWallet _wallet;
        private PlayerInventory _inventory;
        private InteractiveInteraction _iArgs;
        private ulong _chId;
        private ulong _msId;
        public DiscordUser Customer => _customer;
        public ShopCashRegister CashRegister => _cashRegister;
        public MyDiscordClient Client => _client;
        public PlayerWallet Wallet => _wallet;
        public PlayerInventory Inventory => _inventory;
        public DiscordInteraction Args => IArgs.Interaction;
        public InteractiveInteraction IArgs => _iArgs;
        public ShopSession(MyDiscordClient client, DiscordUser customer, PlayerWallet wallet,
         PlayerInventory inventory, ShopCashRegister cashRegister, Action<ShopSession> onExit)
        {
            _client = client;
            _cashRegister = cashRegister;
            _customer = customer;
            _onExit = onExit;
            _wallet = wallet;
            _inventory = inventory;
            _chId = ulong.MinValue;
            _msId = ulong.MinValue;
        }
        public DiscordEmbedBuilder BaseContent(DiscordEmbedBuilder bld = null) =>
            _cashRegister.Default(bld)
            .WithFooter($"{Args.User.Mention}", $"{Args.User.AvatarUrl}")
            .WithAuthor($"{Args.User.Username}#{Args.User.Discriminator}",
             null, $"{Args.User.AvatarUrl}");
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
                var price = $"{_wallet.CurrencyEmoji} {y.Price}";
                return x.AddField($"**{y.Name}**", "**Цена за 1 ед:** " + price, true);
            });
            return emb;

        }
        private DiscordSelectComponent GetSelector(IEnumerable<ShopItem> list = null)
        {

            var items = (list ?? _cashRegister.GetShopItems())
            .Select(x => new DiscordSelectComponentOption(x.Name, x.Name, $"{x.Price}",
                false, new DiscordComponentEmoji(_wallet.CurrencyEmojiId)));
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
        private const bool isEphemeral = false;
        public async Task Respond(DiscordInteractionResponseBuilder bld = default)
        {
            await Args.CreateResponseAsync(GetIRT(), bld?.AsEphemeral(isEphemeral));
            if (_chId == ulong.MinValue)
                _chId = (await Args.GetOriginalResponseAsync()).ChannelId;
            if (_msId == ulong.MinValue)
                _msId = (await Args.GetOriginalResponseAsync()).Id;
        }

        private IDialogueNet DialogNetwork(ShopItem item)
        {
            switch (item.Category)
            {
                case ShopItemCategory.Egg:
                    return new BuyingStepsForEgg(this, item);
                case ShopItemCategory.SatiationCarcass:
                case ShopItemCategory.Carcass:
                case ShopItemCategory.Plant:
                default:
                    return new BuyingStepsForFood(this, item);

                    //default:
                    //    throw new NotImplementedException();
            }
        }
        private async Task ItemSelected(ShopItem item)
        {
            var chain = DialogNetwork(item);
            await Common.RunNetwork(chain);
        }
        public Task<InteractiveInteraction> GetInteraction(
         IEnumerable<DiscordActionRowComponent> components) => 
         GetInteraction(x => x.AnyComponents(components));
        public async Task<InteractiveInteraction> GetInteraction(
         Func<InteractiveInteraction, bool> checker)
        {
            var theEvent = await _client.ActivityTools.WaitForComponentInteraction(x =>
                 x.User.Id == Args.User.Id && x.Message.ChannelId == _chId
                 && (isEphemeral || x.Message.Id == _msId) && checker(new(x.Interaction)));

            return _iArgs = new(theEvent.Interaction);
        }
        public Task<InteractiveInteraction> GetInteraction() => GetInteraction(_ => true);
        public async Task EnterMenu(DiscordInteraction args)
        {
            _iArgs = new(args);
            try
            {
                var exbtn = new DiscordButtonComponent(ButtonStyle.Danger, "Exit", "Выйти");
                while (true)
                {
                    var shopItems = _cashRegister.GetShopItems();
                    var items = GetSelector(shopItems);
                    var mg = GetResponse(GetShopItems(null, shopItems))
                        .AddComponents(items).AddComponents(exbtn);
                    await Respond(mg);


                    var argv = await GetInteraction(x => x.AnyComponents(mg.Components));

                    if (_iArgs.CompareButton(exbtn))
                        break;


                    await ItemSelected(_iArgs.GetOption(shopItems.ToDictionary(x => x.Name)));

                }

                await Respond(GetResponse(BaseContent().WithDescription("Сессия успешно завершена.")));
                StopSession();

                await Task.Delay(10000);
                await Args.DeleteOriginalResponseAsync();
            }
            catch (TimeoutException)
            {
                var ms = "Сессия завершена по причине привешения времени ожидания взаимодействия.";
                var gld = _client.Client;
                var chnl = await gld.GetChannelAsync(_chId);
                await Args.DeleteOriginalResponseAsync();
                var msgtd = await chnl.SendMessageAsync(GetDResponse(BaseContent().WithDescription(ms)));
                StopSession();
                await msgtd.DeleteAsync();
            }
        }
    }

}
