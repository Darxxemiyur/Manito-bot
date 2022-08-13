using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

using Manito.Discord.Chat.DialogueNet;
using Name.Bayfaderix.Darxxemiyur.Node.Network;

namespace Manito.Discord.Shop
{
    public class BuyingStepsForMeatFood : IDialogueNet
    {
        private ShopItem _food;
        private ShopSession _session;
        private int _quantity;
        public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;
        public NextNetworkInstruction GetStartingInstruction(object payload) => GetStartingInstruction();

        public NextNetworkInstruction GetStartingInstruction() => new(SelectQuantity, NextNetworkActions.Continue);
        public BuyingStepsForMeatFood(ShopSession session, ShopItem food)
        {
            _session = session;
            _food = food;
        }

        private async Task<NextNetworkInstruction> SelectQuantity(NetworkInstructionArguments args)
        {
            var btns = Common.Generate;
            var ms1 = $"Выберите количество {_food.Name}";
            var price = _food.Price;

            var qua = await Common.GetQuantity(new[] { -5, -2, 1, 2, 5 }, new[] { 1, 10, 100 }, _session,
             async (x, y) => y < 0 || await _session.Wallet.CanAfford((x + y) * price),
             async x => _session.GetResponse(_session.BaseContent()
             .WithDescription($"{ms1}\nВыбранное количество {x} кг за {x * price}.")), _quantity);

            if (!qua.HasValue)
                return new NextNetworkInstruction(null, NextNetworkActions.Stop);

            if ((_quantity = qua.Value) <= 0)
                return new(SelectQuantity, NextNetworkActions.Continue);

            return new(ExecuteTransaction, NextNetworkActions.Continue);
        }
        private async Task<NextNetworkInstruction> ExecuteTransaction(NetworkInstructionArguments args)
        {
            var wallet = _session.Wallet;
            var inventory = _session.Inventory;
            var price = _quantity * _food.Price;

            if (!await wallet.CanAfford(price))
                return new NextNetworkInstruction(ForceChange, NextNetworkActions.Continue);

            await wallet.Withdraw(price, $"Покупка {_food.Name} за {_food.Price} в кол-ве {_quantity} за {price}");
            await inventory.AddItem(x => (x.ItemType, x.Owner, x.Quantity)
             = ($"{_food.Category}", _session.Customer.Id, _quantity));

            return new NextNetworkInstruction(null, NextNetworkActions.Stop);
        }
        private async Task<NextNetworkInstruction> ForceChange(NetworkInstructionArguments args)
        {
            var price = _quantity * _food.Price;
            var ms1 = $"Вы не можете позволить {_quantity} {_food.Name} за {price}.";
            var ms2 = $"Пожалуйста измените выбранное количество {_food.Name} и попробуйте снова.";
            var rsp = _session.GetResponse(_session.BaseContent().WithDescription($"{ms1}\n{ms2}"));

            var cancel = new DiscordButtonComponent(ButtonStyle.Danger, "Cancel", "Отмена");
            var chnamt = new DiscordButtonComponent(ButtonStyle.Primary, "Back", "Изменить кол-во");
            rsp.AddComponents(cancel, chnamt);

            await _session.Respond(rsp);

            var argv = await _session.GetInteraction();

            if (argv.CompareButton(chnamt))
                return new(SelectQuantity, NextNetworkActions.Continue);

            return new(null, NextNetworkActions.Stop);
        }
    }
}