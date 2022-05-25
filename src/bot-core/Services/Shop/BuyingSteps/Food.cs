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

namespace Manito.Discord.Shop
{
    public class BuyingStepsForFood : IBuyingSteps
    {
        private Dictionary<string, BuyStep> _buySteps;
        private ShopItem _food;
        private ShopSession _session;
        private int _quantity;
        public NextInstruction GetStartingInstruction() => new(_default, NextActions.Continue);
        private string _default;
        public BuyingStepsForFood(ShopSession session, ShopItem food)
        {
            _session = session;
            _food = food;
            var steps = ConstructBuySteps();

            _buySteps = steps.ToDictionary(x => x.GetMethodInfo().Name, x => x);
            _default = steps.First().GetMethodInfo().Name;
        }
        private IEnumerable<BuyStep> ConstructBuySteps()
        {
            yield return SelectQuantity;
            yield return Buy;
            yield return ExecuteTransaction;
            yield return ForceChange;
        }

        private async Task<NextInstruction> SelectQuantity(InstructionArguments args)
        {
            while (true)
            {
                var btns = BuyingStepsCommon.Generate(new[] { -5, 1, 2, 5 }, new[] { 1, 10, 100 });
                var ms1 = $"Выберите количество {_food.Name}";
                var ms2 = $"Выбранное количество {_quantity} ед.";
                var mg2 = _session.GetResponse(_session.BaseContent().WithDescription($"{ms1}\n{ms2}"));

                foreach (var row in btns)
                    mg2.AddComponents(row);


                mg2.AddComponents(
                    new DiscordButtonComponent(ButtonStyle.Danger, "Exit", "Назад"),
                    new DiscordButtonComponent(ButtonStyle.Success, "Submit", "Выбрать"));


                await _session.Respond(mg2);


                _session.Args = (await _session.Client.ActivityTools.WaitForComponentInteraction(x =>
                      x.Message.ChannelId == args.ChannelId && x.User.Id == _session.Args.User.Id &&
                      mg2.Components.SelectMany(y => y.Components)
                      .Any(y => x.Interaction.Data.CustomId == y.CustomId))).Interaction;

                if (_session.Args.Data.CustomId == "Exit" || _session.Args.Data.CustomId == "Submit")
                    break;

                var pressed = btns.SelectMany(x => x).First(x => x.CustomId == _session.Args.Data.CustomId);

                var change = int.Parse(pressed.Label);

                _quantity = Math.Clamp(_quantity + change, 0, int.MaxValue);
            }

            return new NextInstruction(nameof(ExecuteTransaction), NextActions.Continue);
        }
        private async Task<NextInstruction> ExecuteTransaction(InstructionArguments args)
        {
            var economy = _session.Economy;
            var customer = _session.Customer;
            var inventory = _session.Client.Service.Inventory;
            var price = _quantity * _food.Price;

            if (!await economy.CanAfford(customer.Id, price))
                return new NextInstruction(nameof(ForceChange), NextActions.Continue);

            await economy.Withdraw(customer.Id, price);
            //inventory.AddItem(customer, null);

            return new NextInstruction(null, NextActions.Success);
        }
        private async Task<NextInstruction> ForceChange(InstructionArguments args)
        {
            var price = _quantity * _food.Price;
            var ms1 = $"Вы не можете позволить {_quantity} {_food.Name} за {price}.";
            var ms2 = $"Пожалуйста измените выбранное количество {_food.Name} и попробуйте снова.";
            var rsp = _session.GetResponse(_session.BaseContent().WithDescription($"{ms1}\n{ms2}"));

            rsp.AddComponents(
                new DiscordButtonComponent(ButtonStyle.Danger, "Cancel", "Отмена"),
                new DiscordButtonComponent(ButtonStyle.Primary, "Back", "Изменить кол-во"));

            await _session.Respond(rsp);
            
            var argv = await _session.Client.ActivityTools.WaitForComponentInteraction(x =>
                   x.Message.ChannelId == args.ChannelId && x.User.Id == _session.Args.User.Id);

            _session.Args = argv.Interaction;

            var btn = _session.Args.Data.CustomId;

            if (btn == "Back")
                return new(nameof(SelectQuantity), NextActions.Continue);
            else if (btn == "Cancel")
                return new(null, NextActions.Success);

            throw new Exception();
        }
        private async Task<NextInstruction> Buy(InstructionArguments args)
        {
            var price = _quantity * _food.Price;
            var rsp = _session.GetResponse(_session.BaseContent()
            .WithDescription($"Купить {_quantity} {_food.Name} за {price}?"));
            rsp.AddComponents(
                    new DiscordButtonComponent(ButtonStyle.Danger, "Cancel", "Отмена"),
                    new DiscordButtonComponent(ButtonStyle.Primary, "Back", "Изменить кол-во"),
                    new DiscordButtonComponent(ButtonStyle.Success, "Buy", "Купить"));

            await _session.Respond(rsp);


            var argv = await _session.Client.ActivityTools.WaitForComponentInteraction(x =>
                   x.Message.ChannelId == args.ChannelId && x.User.Id == _session.Args.User.Id);

            _session.Args = argv.Interaction;

            var btn = _session.Args.Data.CustomId;

            if (btn == "Buy")
                return new(nameof(ExecuteTransaction), NextActions.Continue);
            else if (btn == "Back")
                return new(nameof(SelectQuantity), NextActions.Continue);
            else if (btn == "Cancel")
                return new(null, NextActions.Success);

            throw new Exception();
        }

        #region  LIST IMPOSING
        public BuyStep this[string index] => _buySteps[index];

        public int Count => _buySteps.Count;

        public IEnumerable<string> Keys => _buySteps.Keys;

        public IEnumerable<BuyStep> Values => _buySteps.Values;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_buySteps).GetEnumerator();
        }

        IReadOnlyDictionary<string, BuyStep> IBuyingSteps.GetBuySteps()
        {
            return this;
        }

        public bool ContainsKey(string key)
        {
            return _buySteps.ContainsKey(key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out BuyStep value)
        {
            return _buySteps.TryGetValue(key, out value);
        }

        IEnumerator<KeyValuePair<string, BuyStep>> IEnumerable<KeyValuePair<string, BuyStep>>.GetEnumerator()
        {
            return _buySteps.GetEnumerator();
        }
        #endregion
    }
}