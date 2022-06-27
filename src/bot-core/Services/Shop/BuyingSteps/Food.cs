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

namespace Manito.Discord.Shop
{
    public class BuyingStepsForFood : IDialogueNet
    {
        private ShopItem _food;
        private ShopSession _session;
        private int _quantity;
        public NextNetInstruction GetStartingInstruction() => new(SelectQuantity, NextNetActions.Continue);
        public BuyingStepsForFood(ShopSession session, ShopItem food)
        {
            _session = session;
            _food = food;
        }

        private async Task<NextNetInstruction> SelectQuantity(InstructionArguments args)
        {
            while (true)
            {
                var btns = Common.Generate(new[] { -5, 1, 2, 5 }, new[] { 1, 10, 100 });
                var ms1 = $"Выберите количество {_food.Name}";
                var ms2 = $"Выбранное количество {_quantity} ед.";
                var mg2 = _session.GetResponse(_session.BaseContent().WithDescription($"{ms1}\n{ms2}"));

                foreach (var row in btns)
                    mg2.AddComponents(row);

                var exbtn = new DiscordButtonComponent(ButtonStyle.Danger, "Exit", "Назад");
                var sbmbtn = new DiscordButtonComponent(ButtonStyle.Success, "Submit", "Выбрать");
                mg2.AddComponents(exbtn, sbmbtn);

                await _session.Respond(mg2);

                await _session.GetInteraction(mg2.Components);

                if (_session.IArgs.CompareButton(exbtn))
                    return new NextNetInstruction(null, NextNetActions.Success);

                if (_session.IArgs.CompareButton(sbmbtn) && _quantity > 0)
                    break;

                var pressed = _session.IArgs.GetButton(btns.SelectMany(x => x).ToDictionary(x => x.CustomId));
                var change = int.Parse(pressed.Label);
                _quantity = Math.Clamp(_quantity + change, 0, int.MaxValue);

            }
            return new NextNetInstruction(ExecuteTransaction, NextNetActions.Continue);
        }
        private async Task<NextNetInstruction> ExecuteTransaction(InstructionArguments args)
        {
            var wallet = _session.Wallet;
            var inventory = _session.Inventory;
            var price = _quantity * _food.Price;

            if (!await wallet.CanAfford(price))
                return new NextNetInstruction(ForceChange, NextNetActions.Continue);

            await wallet.Withdraw(price, $"Покупка {_food.Name} за {_food.Price} в кол-ве {_quantity} за {price}");
            await inventory.AddItem(x => (x.ItemType, x.Owner, x.Quantity)
             = (_food.Name, _session.Customer.Id, _quantity));

            return new NextNetInstruction(null, NextNetActions.Success);
        }
        private async Task<NextNetInstruction> ForceChange(InstructionArguments args)
        {
            var price = _quantity * _food.Price;
            var ms1 = $"Вы не можете позволить {_quantity} {_food.Name} за {price}.";
            var ms2 = $"Пожалуйста измените выбранное количество {_food.Name} и попробуйте снова.";
            var rsp = _session.GetResponse(_session.BaseContent().WithDescription($"{ms1}\n{ms2}"));

            rsp.AddComponents(
                new DiscordButtonComponent(ButtonStyle.Danger, "Cancel", "Отмена"),
                new DiscordButtonComponent(ButtonStyle.Primary, "Back", "Изменить кол-во"));

            await _session.Respond(rsp);

            var argv = await _session.GetInteraction();

            var btn = _session.Args.Data.CustomId;

            if (btn == "Back")
                return new(SelectQuantity, NextNetActions.Continue);
            else if (btn == "Cancel")
                return new(null, NextNetActions.Success);

            throw new Exception();
        }
        private async Task<NextNetInstruction> Buy(InstructionArguments args)
        {
            var price = _quantity * _food.Price;
            var rsp = _session.GetResponse(_session.BaseContent()
            .WithDescription($"Купить {_quantity} {_food.Name} за {price}?"));
            rsp.AddComponents(
                    new DiscordButtonComponent(ButtonStyle.Danger, "Cancel", "Отмена"),
                    new DiscordButtonComponent(ButtonStyle.Primary, "Back", "Изменить кол-во"),
                    new DiscordButtonComponent(ButtonStyle.Success, "Buy", "Купить"));

            await _session.Respond(rsp);


            var argv = await _session.GetInteraction();

            var btn = _session.Args.Data.CustomId;

            if (btn == "Buy")
                return new(ExecuteTransaction, NextNetActions.Continue);
            else if (btn == "Back")
                return new(SelectQuantity, NextNetActions.Continue);
            else if (btn == "Cancel")
                return new(null, NextNetActions.Success);

            throw new Exception();
        }
    }
}