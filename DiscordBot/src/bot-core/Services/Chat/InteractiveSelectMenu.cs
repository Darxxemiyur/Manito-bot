using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Manito.Discord.Chat.DialogueNet;
using Microsoft.Extensions.DependencyInjection;
using Name.Bayfaderix.Darxxemiyur.Node.Network;


namespace Manito.Discord.Client
{

    /// <summary>
    /// Single net dialogue based Item selector.
    /// </summary>
    public class InteractiveSelectMenu<TItem> : IDialogueNet
    {
        private DialogueNetSession _session;
        private const int max = 25;
        private const int rows = 5;
        private int _page;
        private int _pageCount;
        private int _leftsp;
        private DiscordWebhookBuilder _msg;
        private DiscordComponent[] _btnDef;
        private DiscordButtonComponent _firstList;
        private DiscordButtonComponent _prevList;
        private DiscordButtonComponent _exBtn;
        private DiscordButtonComponent _nextList;
        private DiscordButtonComponent _latterList;
        private string _navPrefix;
        private string _itmPrefix;
        private string _othPrefix;
        private IEnumerable<IItemDescriptor<TItem>> _selections;
        public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;
        public InteractiveSelectMenu(DialogueNetSession session,
         IEnumerable<IItemDescriptor<TItem>> itemList)
        {
            _session = session;
            _selections = itemList;
            _navPrefix = "nav";
            _itmPrefix = "item";
        }

        public async Task<IItemDescriptor<TItem>> EvaluateItem() =>
         (IItemDescriptor<TItem>)await NetworkCommon.RunNetwork(this);

        private async Task<NextNetworkInstruction> Initiallize(NetworkInstructionArgument args)
        {
            _firstList = new DiscordButtonComponent(ButtonStyle.Success, $"{_navPrefix}_firstBtn",
             "Перв. стр", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀️")));
            _prevList = new DiscordButtonComponent(ButtonStyle.Success, $"{_navPrefix}_prevBtn",
             "Пред. стр", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⬅️")));
            _exBtn = new DiscordButtonComponent(ButtonStyle.Danger, $"{_navPrefix}_exitBtn",
             "Закрыть", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖️")));
            _nextList = new DiscordButtonComponent(ButtonStyle.Success, $"{_navPrefix}_nextBtn",
             "След. стр", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➡️")));
            _latterList = new DiscordButtonComponent(ButtonStyle.Success, $"{_navPrefix}_latterBtn",
             "Посл. стр", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶️")));
            _btnDef = new DiscordComponent[] { _firstList, _prevList, _exBtn, _nextList, _latterList };

            return new(PrintActions, NextNetworkActions.Continue);
        }
        private async Task<NextNetworkInstruction> PrintActions(NetworkInstructionArgument args)
        {
            _leftsp = max - _btnDef.Length;

            _msg = new DiscordWebhookBuilder();
            var emb = new DiscordEmbedBuilder();

            var invCount = _selections.Count();

            var pages = _selections.Select((x, y) => x.SetGlobalDisplayedOrder(y)).Chunk(_leftsp);

            _pageCount = Math.Max(pages.Count() - 1, 0);

            _page = Math.Clamp(_page, 0, _pageCount);

            IEnumerable<IItemDescriptor<TItem>> itms = pages.ElementAtOrDefault(_page)?
            .Select((x, y) => x.SetLocalDisplayedOrder(y));

            var btns = itms?.Where(x => x.HasButton()).Select(x =>
                new DiscordButtonComponent(ButtonStyle.Primary,
                $"{_itmPrefix}_{x.GetButtonId()}", x.GetButtonName()))
                ?? Enumerable.Empty<DiscordButtonComponent>();

            btns = btns.Concat(Enumerable.Range(1, _leftsp - btns.Count()).Select(x =>
                new DiscordButtonComponent(ButtonStyle.Secondary, $"{x}dummy",
                " ** ** ** ** ** ** ", true)));

            emb.WithFooter($"Всего предметов: {invCount}\nСтраница {_page + 1} из {_pageCount + 1}");

            if (_page == 0)
            {
                _firstList.Disable();
                _prevList.Disable();
            }
            else
            {
                _firstList.Enable();
                _prevList.Enable();
            }

            if (_page == _pageCount)
            {
                _nextList.Disable();
                _latterList.Disable();
            }
            else
            {
                _nextList.Enable();
                _latterList.Enable();
            }

            foreach (var btnsr in btns.Concat(_btnDef).Chunk(rows))
                _msg.AddComponents(btnsr);

            await _session.Args.EditOriginalResponseAsync(_msg.AddEmbed(emb));

            return new(WaitForResponse, itms);
        }
        private async Task<NextNetworkInstruction> WaitForResponse(NetworkInstructionArgument args)
        {
            var inv = (IEnumerable<IItemDescriptor<TItem>>)args.Payload;

            var resp = await _session.GetInteraction(_msg.Components);


            if (resp.ButtonId.StartsWith(_itmPrefix))
                return new(ReturnItem, NextNetworkActions.Continue, (resp, inv));

            if (resp.ButtonId.StartsWith(_navPrefix))
                return new(ReturnNoItem, NextNetworkActions.Continue, resp);


            return new();
        }
        private async Task<NextNetworkInstruction> ReturnItem(NetworkInstructionArgument args)
        {
            var resp = ((InteractiveInteraction, IEnumerable<IItemDescriptor<TItem>>))args.Payload;
            
            await _session.Respond(InteractionResponseType.DeferredMessageUpdate);

            var item = resp.Item2.FirstOrDefault(x => resp
                .Item1.ButtonId.Contains($"_{x.GetButtonId()}"));

            return new(null, NextNetworkActions.Stop, item);
        }
        private async Task<NextNetworkInstruction> ReturnNoItem(NetworkInstructionArgument args)
        {
            var resp = (InteractiveInteraction)args.Payload;

            if (resp.CompareButton(_exBtn)) return new();
            
            await _session.Respond(InteractionResponseType.DeferredMessageUpdate);

            if (resp.CompareButton(_firstList)) _page = 0;

            if (resp.CompareButton(_prevList)) _page--;

            if (resp.CompareButton(_nextList)) _page++;

            if (resp.CompareButton(_latterList)) _page = _pageCount;

            return new(PrintActions);
        }

        public NextNetworkInstruction GetStartingInstruction(object payload) => GetStartingInstruction();
        public NextNetworkInstruction GetStartingInstruction() => new(Initiallize);
    }
    public interface IItemDescriptor<TItem>
    {
        string GetButtonName();
        string GetButtonId();
        bool HasButton();
        bool HasField();
        string GetFieldName();
        string GetFieldBody();
        IItemDescriptor<TItem> SetGlobalDisplayedOrder(int i);
        IItemDescriptor<TItem> SetLocalDisplayedOrder(int i);
        int GetGlobalDisplayOrder();
        int GetLocalDisplayOrder();
        TItem GetCarriedItem();
    }
}
