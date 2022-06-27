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
using Manito.Discord.Chat.DialogueNet;

namespace Manito.Discord.Inventory
{
    public class ListItems : IDialogueNet
    {
        private InventorySession _session;

        public ListItems(InventorySession session, int startPage)
        {
            _session = session;
            _page = startPage;
        }
        private const int max = 25;
        private const int rows = 5;
        private int _page;
        private async Task<NextNetInstruction> ListTheItems(InstructionArguments args)
        {
            var exBtn = new DiscordButtonComponent(ButtonStyle.Danger, "exitBtn", "Закрыть",
             false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖️")));
            var firstList = new DiscordButtonComponent(ButtonStyle.Success, "firstBtn", "Перв. стр",
             false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀️")));
            var prevList = new DiscordButtonComponent(ButtonStyle.Success, "prevBtn", "Пред. стр",
             false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⬅️")));
            var nextList = new DiscordButtonComponent(ButtonStyle.Success, "nextBtn", "След. стр",
             false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➡️")));
            var latterList = new DiscordButtonComponent(ButtonStyle.Success, "latterBtn", "Посл. стр",
             false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶️")));
            var def = new DiscordComponent[] { firstList, prevList, exBtn, nextList, latterList };
            var leftsp = max - def.Length;

            while (true)
            {
                var msg = new DiscordInteractionResponseBuilder();
                var emb = new DiscordEmbedBuilder();

                var inv = _session.PInventory.GetInventoryItems();
                var invCount = inv.Count();

                var pages = inv.Select((x, y) => (x, y)).Chunk(leftsp);

                var pageCount = Math.Max(pages.Count() - 1, 0);

                _page = Math.Clamp(_page, 0, pageCount);

                var btns = Enumerable.Empty<DiscordComponent>();
                if (invCount > 0)
                {
                    var list = pages.ElementAtOrDefault(_page);

                    foreach (var (x, y) in list)
                        emb.AddField($"Предмет №{y}", $"{x.ItemType} x{x.Quantity}", true);

                    btns = list.Select(x => new DiscordButtonComponent(ButtonStyle.Primary,
                       $"openitem{x.x.Id}", $"{x.x.ItemType}"));
                }
                else
                {
                    emb.WithDescription("У вас пусто в инвентаре :(");
                }

                btns = btns.Concat(Enumerable.Range(1, leftsp - btns.Count()).Select(x =>
                 new DiscordButtonComponent(ButtonStyle.Secondary, $"{x}dummy",
                 " ** ** ** ** ** ** ", true)));

                emb.WithFooter($"Всего предметов: {invCount}\nСтраница {_page + 1} из {pageCount + 1}");

                if (_page == 0)
                {
                    firstList.Disable();
                    prevList.Disable();
                }
                else
                {
                    firstList.Enable();
                    prevList.Enable();
                }

                if (_page == pageCount)
                {
                    nextList.Disable();
                    latterList.Disable();
                }
                else
                {
                    nextList.Enable();
                    latterList.Enable();
                }

                foreach (var btnsr in btns.Concat(def).Chunk(rows))
                    msg.AddComponents(btnsr);

                await _session.Respond(msg.AddEmbed(emb));

                var resp = await _session.GetInteraction(msg.Components);

                if (resp.CompareButton(firstList))
                {
                    _page = 0;
                    continue;
                }

                if (resp.CompareButton(prevList))
                {
                    _page--;
                    continue;
                }

                if (resp.CompareButton(nextList))
                {
                    _page++;
                    continue;
                }

                if (resp.CompareButton(latterList))
                {
                    _page = pageCount;
                    continue;
                }

                if (resp.CompareButton(exBtn))
                {
                    await _session.QuitSession();
                    break;
                }

                await _session.PInventory.RemoveItem(inv
                 .FirstOrDefault(x => resp.Interaction
                 .Data.CustomId.Contains($"{x.Id}")));
            }

            return new(null, NextNetActions.Success);
        }

        public NextNetInstruction GetStartingInstruction() => new(ListTheItems, NextNetActions.Continue);
    }
}