using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Enums;

using Manito.Discord.Chat.DialogueNet;
using Manito.Discord.ChatNew;

using Microsoft.Extensions.DependencyInjection;

using Name.Bayfaderix.Darxxemiyur.Node.Network;


namespace Manito.Discord.Client
{
	/// <summary>
	/// Single net dialogue based Item selector.
	/// </summary>
	public class InteractiveSelectMenu<TItem> : IDialogueNet
	{
		private InteractionPuller _puller;
		private SessionResponder _responder;
		private const int max = 25;
		private const int rows = 5;
		private int Page {
			get => _paginater.Page; set => _paginater.Page = value;
		}
		private int PageCount => _paginater.GetPages;
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
		public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;
		private IPageReturner<TItem> _paginater;
		public InteractiveSelectMenu(InteractionPuller puller, SessionResponder responder, IPageReturner<TItem> paginater)
		{
			_responder = responder;
			_puller = puller;
			_paginater = paginater;
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

			_paginater.PerPage = _leftsp;

			var invCount = _paginater.Total;

			Page = Page;


			IEnumerable<IItemDescriptor<TItem>> itms = _paginater.ListablePage
				.Select((x, y) => x.SetLocalDisplayedOrder(y));

			var btns = itms?.Where(x => x.HasButton()).Select(x =>
				new DiscordButtonComponent(ButtonStyle.Primary,
				$"{_itmPrefix}_{x.GetButtonId()}", x.GetButtonName()))
				?? Enumerable.Empty<DiscordButtonComponent>();

			btns = btns.Concat(Enumerable.Range(1, _leftsp - btns.Count()).Select(x =>
				new DiscordButtonComponent(ButtonStyle.Secondary, $"{x}dummy",
				" ** ** ** ** ** ** ", true)));

			emb.WithFooter($"Всего предметов: {invCount}\nСтраница {Page} из {PageCount}");

			if (Page <= 1)
			{
				_firstList.Disable();
				_prevList.Disable();
			}
			else
			{
				_firstList.Enable();
				_prevList.Enable();
			}

			if (Page >= PageCount)
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

			await _responder.SendMessage(_msg.AddEmbed(emb));

			return new(WaitForResponse, itms);
		}
		private async Task<NextNetworkInstruction> WaitForResponse(NetworkInstructionArgument args)
		{
			var inv = (IEnumerable<IItemDescriptor<TItem>>)args.Payload;

			//var resp = await _puller.GetComponentInteraction(_msg.Components);
			var resp = await _puller.GetComponentInteraction();


			if (resp.ButtonId.StartsWith(_itmPrefix))
				return new(ReturnItem, NextNetworkActions.Continue, (resp, inv));

			if (resp.ButtonId.StartsWith(_navPrefix))
				return new(ReturnNoItem, NextNetworkActions.Continue, resp);


			return new();
		}
		private async Task<NextNetworkInstruction> ReturnItem(NetworkInstructionArgument args)
		{
			var resp = ((InteractiveInteraction, IEnumerable<IItemDescriptor<TItem>>))args.Payload;

			await _responder.DoLaterReply();

			var item = resp.Item2.FirstOrDefault(x => resp
				.Item1.ButtonId.Contains($"_{x.GetButtonId()}"));

			return new(null, NextNetworkActions.Stop, item);
		}
		private async Task<NextNetworkInstruction> ReturnNoItem(NetworkInstructionArgument args)
		{
			var resp = (InteractiveInteraction)args.Payload;

			if (resp.CompareButton(_exBtn))
				return new();

			await _responder.DoLaterReply();

			if (resp.CompareButton(_firstList))
				Page = 0;

			if (resp.CompareButton(_prevList))
				Page--;

			if (resp.CompareButton(_nextList))
				Page++;

			if (resp.CompareButton(_latterList))
				Page = PageCount;

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

	public interface IPageReturner<TItem>
	{
		IList<IItemDescriptor<TItem>> ListablePage {
			get;
		}

		/// <summary>
		/// Displayed on pages
		/// </summary>
		/// <value></value>
		int PerPage {
			get; set;
		}

		/// <summary>
		/// Total on current page amount
		/// </summary>
		/// <value></value>
		int OnPage {
			get;
		}

		/// <summary>
		/// Total item amount
		/// </summary>
		/// <value></value>
		int Total {
			get;
		}

		/// <summary>
		/// Total pages amount
		/// </summary>
		/// <value></value>
		int GetPages {
			get;
		}

		/// <summary>
		/// Current page. Set, or Get
		/// </summary>
		/// <value></value>
		int Page {
			get; set;
		}
	}
	public class EnumerablePageReturner<TItem> : IPageReturner<TItem>
	{
		public IList<IItemDescriptor<TItem>> ListablePage => _list
			.Skip((Page - 1) * PerPage).Take(PerPage).Select(x => _convert(x)).ToList();
		public Int32 PerPage { get; set; } = 25;
		public Int32 OnPage => ListablePage.Count;
		public Int32 Total => _list.Count;
		public Int32 GetPages => Math.Max((int)Math.Ceiling((float)Total / PerPage), 1);
		private int _page;
		public int Page {
			get => _page; set => _page = Math.Clamp(value, 1, GetPages);
		}
		private List<TItem> _list;
		private Func<TItem, IItemDescriptor<TItem>> _convert;
		public EnumerablePageReturner(List<TItem> list, Func<TItem, IItemDescriptor<TItem>> converter)
		{
			_list = list;
			_convert = converter;
		}
	}
	public class QueryablePageReturner<TItem> : IPageReturner<TItem>
	{
		private IEnumerable<TItem> GetQueryablePage() => Querrier.GetSection(PerPage * (Page - 1), PerPage);
		public IList<IItemDescriptor<TItem>> ListablePage => GetQueryablePage()
		.ToList().ConvertAll(x => Querrier.Convert(x));
		public QueryablePageReturner(IQuerrier<TItem> querrier)
		{
			Querrier = querrier;
		}
		public int GetPages => Math.Max((int)Math.Ceiling((float)Total / PerPage), 1);
		private int _page;
		public int Page {
			get => _page; set => _page = Math.Clamp(value, 1, GetPages);
		}
		public int PerPage { get; set; } = 25;
		public int OnPage => GetQueryablePage().Count();
		public int Total => Querrier.GetTotalCount();
		public IQuerrier<TItem> Querrier {
			get;
		}
	}
	public interface IQuerrier<TItem>
	{
		IEnumerable<TItem> GetSection(int skip, int take);
		int GetPages(int perPage);
		int GetTotalCount();
		IItemDescriptor<TItem> Convert(TItem item);
	}
}
