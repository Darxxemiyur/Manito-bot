using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using Manito.Discord.Client;


namespace Manito.Discord.Inventory
{
    public class InventoryCommands
    {
        private const string Locale = "ru";
        private InventorySystem _inventory;
        public InventoryCommands(InventorySystem inventory) => _inventory = inventory;
        public Func<DiscordInteraction, Task> Search(DiscordInteraction command)
        {
            foreach (var item in GetCommands())
            {
                if (command.Data.Name.Contains(item.Name))
                {
                    foreach (var subItem in GetSubCommands())
                    {
                        if (command.Data.Options.First().Name.Contains(subItem.Item1.Name))
                            return subItem.Item2;
                    }
                }
            }
            return null;
        }
        private Dictionary<string, string> GetLoc(string trans) => new Dictionary<string, string>() { { Locale, trans } };
        private IEnumerable<(DiscordApplicationCommandOption, Func<DiscordInteraction, Task>)> GetSubCommands()
        {
            yield return (new DiscordApplicationCommandOption("open", "Open inventory",
             ApplicationCommandOptionType.SubCommand, null, null, new[] {
                 new DiscordApplicationCommandOption("page","Page",
                 ApplicationCommandOptionType.Integer, false),
             },
             name_localizations: GetLoc("открыть"),
             description_localizations: GetLoc("Открыть инвентарь")),
             ShowInventory);

            yield return (new DiscordApplicationCommandOption("use", "Use item",
             ApplicationCommandOptionType.SubCommand, null, null, new[] {
                 new DiscordApplicationCommandOption("number","Item number",
                 ApplicationCommandOptionType.Integer, true),
             },
             name_localizations: GetLoc("использовать"),
             description_localizations: GetLoc("Использовать предмет")),
             UseItem);
        }
        public IEnumerable<DiscordApplicationCommand> GetCommands()
        {
            yield return new DiscordApplicationCommand("inventory", "Inventory",
             GetSubCommands().Select(x => x.Item1), true,
             ApplicationCommandType.SlashCommand,
             GetLoc("инвентарь"), GetLoc("Инвентарь"));

        }

        private async Task UseItem(DiscordInteraction args)
        {
            var tools = new AppArgsTools(args);
            var numArg = tools.AddReqArg("number");

            if (!tools.DoHaveReqArgs())
                return;

            var target = args.User;
            var inventory = new PlayerInventory(_inventory, target);

            var num = tools.GetReq().GetIntArg(numArg);

            var items = inventory.GetInventoryItems().Select(x => x);

            var saNum = Math.Clamp(num, 1, items.Count());

            var emb = new DiscordEmbedBuilder();

            if (num != saNum)
            {
                emb.WithDescription("Предмет не использован!\nВы ввели неправильный id!");
            }
            else
            {
                var item = items.ElementAt(saNum - 1);
                inventory.TestRemoveItem(item);
                emb.WithDescription($"Использован предмет {item.ItemType} с id{item.Id}");
            }

            var msg = new DiscordInteractionResponseBuilder();
            msg.AddEmbed(emb);
            await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
        }

        /// <summary>
        /// Show user's inventory
        /// </summary>
        /// <returns></returns>
        private async Task ShowInventory(DiscordInteraction args)
        {
            var tools = new AppArgsTools(args);
            var numArg = tools.AddOptArg("page");

            var target = args.User;
            var inventory = new PlayerInventory(_inventory, target);

            var emb = new DiscordEmbedBuilder();

            var items = inventory.GetInventoryItems().Select((x, y) => (x, y));
            var itemsPaged = items.Chunk(25);
            var page = 1;

            var pages = itemsPaged.Count();

            if (tools.AnyOptArgs())
                page = Math.Clamp(tools.GetOptional().GetIntArg(numArg), 1, pages);

            foreach (var item in itemsPaged.ElementAt(page - 1))
            {
                emb.AddField($"Предмет №{item.y}", item.x.ItemType, true);
            }

            emb.WithFooter($"Всего предметов: {items.Count()}\nСтраница {page} из {pages}");

            var msg = new DiscordInteractionResponseBuilder();
            msg.AddEmbed(emb);
            await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);

        }
    }

}