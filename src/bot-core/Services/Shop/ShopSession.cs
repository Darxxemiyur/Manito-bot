using System;
using System.Threading.Tasks;
using System.Linq;

using DSharpPlus;
using DSharpPlus.Entities;

using Manito.Discord.Client;
using Name.Bayfaderix.Darxxemiyur.Common;
using System.Collections.Generic;
using Manito.Discord.Economy;
using DSharpPlus.EventArgs;
using Manito.Discord.Chat.DialogueNet;
using Manito.Discord.Inventory;
using Name.Bayfaderix.Darxxemiyur.Node.Network;

namespace Manito.Discord.Shop
{

    public class ShopSession : DialogueNetSession
    {
        private ShopCashRegister _cashRegister;
        private Action<ShopSession> _onExit;
        private PlayerWallet _wallet;
        private PlayerInventory _inventory;
        public DiscordUser Customer => _user;
        public ShopCashRegister CashRegister => _cashRegister;
        public PlayerWallet Wallet => _wallet;
        public PlayerInventory Inventory => _inventory;
        public ShopSession(InteractiveInteraction iargs, MyDiscordClient client, DiscordUser customer,
         PlayerWallet wallet, PlayerInventory inventory, ShopCashRegister cashRegister,
          Action<ShopSession> onExit) : base(iargs, client, customer, false)
        {
            _cashRegister = cashRegister;
            _onExit = onExit;
            _wallet = wallet;
            _inventory = inventory;
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
        private IDialogueNet DialogNetwork(ShopItem item)
        {
            switch (item.Category)
            {
                case ShopItemCategory.Egg:
                    return new BuyingStepsForEgg(this, item);
                case ShopItemCategory.SatiationCarcass:
                case ShopItemCategory.Carcass:
                    return new BuyingStepsForMeatFood(this, item);
                case ShopItemCategory.Plant:
                    return new BuyingStepsForPlantFood(this, item);
                default:
                    return new BuyingStepsForPlantFood(this, item);

                    //default:
                    //    throw new NotImplementedException();
            }
        }
        private async Task ItemSelected(ShopItem item)
        {
            var chain = DialogNetwork(item);
            await NetworkCommon.RunNetwork(chain);
        }
        public async Task EnterMenu()
        {
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

                    if (_iargs.CompareButton(exbtn))
                        break;


                    await ItemSelected(_iargs.GetOption(shopItems.ToDictionary(x => x.Name)));

                }

                await Respond(GetResponse(BaseContent().WithDescription("Сессия успешно завершена.")));
                StopSession();

                await Task.Delay(10000);
                await Args.DeleteOriginalResponseAsync();
            }
            catch (TimeoutException)
            {
                var ms = "Сессия завершена по причине привышения времени ожидания взаимодействия.";
                var gld = _client.Client;
                var chnl = await gld.GetChannelAsync(_chId.Value);
                await Args.DeleteOriginalResponseAsync();
                var msgtd = await chnl.SendMessageAsync(GetDResponse(BaseContent().WithDescription(ms)));
                StopSession();
                await msgtd.DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }
    }

}
