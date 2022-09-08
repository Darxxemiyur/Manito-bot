using System;
using System.Threading.Tasks;
using System.Linq;

using DSharpPlus;
using DSharpPlus.Entities;

using Manito.Discord.Client;
using Manito.Discord.Chat.DialogueNet;
using Name.Bayfaderix.Darxxemiyur.Node.Network;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Numerics;
using DSharpPlus.Exceptions;
using Manito.Discord.PatternSystems.Common;

namespace Manito.Discord.PermanentMessage
{
	public class MsgWallPanelWallTranslator : INodeNetwork
	{
		private class Descriptor : IItemDescriptor<MessageWallTranslator>
		{
			private readonly MessageWallTranslator _wallLine;
			public Descriptor(MessageWallTranslator wallLine) => _wallLine = wallLine;
			private int _lid;
			private int _gid;
			public string GetButtonId() => $"Translator{_lid}_{_wallLine.ID}";

			private string GetMyThing(string str) => $"Транслятор {str} ID:<#{_wallLine.ChannelId}>";
			public string GetButtonName() => GetMyThing(_wallLine.MessageWall?.WallName.DoAtMax(80 - GetMyThing("").Length));

			public MessageWallTranslator GetCarriedItem() => _wallLine;

			public string GetFieldBody() => throw new NotImplementedException();

			public string GetFieldName() => throw new NotImplementedException();

			public int GetGlobalDisplayOrder() => _gid;

			public int GetLocalDisplayOrder() => _lid;

			public bool HasButton() => true;

			public bool HasField() => false;

			public IItemDescriptor<MessageWallTranslator> SetGlobalDisplayedOrder(int i)
			{
				_gid = i;
				return this;
			}

			public IItemDescriptor<MessageWallTranslator> SetLocalDisplayedOrder(int i)
			{
				_lid = i;
				return this;
			}
		}
		public class Editor : INodeNetwork
		{
			private MessageWallSession _session;
			private MessageWallTranslator _translator;
			private MsgWallPanelWall.Selector _wallSelector;
			private MsgWallPanelWall.Editor _wallEditor;
			private NextNetworkInstruction _ret;
			public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;
			public Editor(MessageWallSession session, NextNetworkInstruction ret)
			{
				_session = session;
				_ret = ret;
			}
			private async Task<NextNetworkInstruction> ShowOptions(NetworkInstructionArgument args)
			{
				var syncBtn = new DiscordButtonComponent(ButtonStyle.Primary,
				 "sync", "Синхронизировать", _translator.ChannelId == 0);
				var linkChnlBtn = new DiscordButtonComponent(ButtonStyle.Primary, "linkc", "Привязать к каналу");
				var linkWallBtn = new DiscordButtonComponent(ButtonStyle.Primary, "linkw", "Привязать к стене");
				var openWallBtn = new DiscordButtonComponent(ButtonStyle.Primary, "select",
				 "Открыть стену транслятора", _translator.MessageWall == null);
				var remBtn = new DiscordButtonComponent(ButtonStyle.Danger, "remove", "Удалить");
				var exitBtn = new DiscordButtonComponent(ButtonStyle.Danger, "exit", "Выйти");

				var emb = new DiscordEmbedBuilder();

				emb.WithAuthor("Транслятор стены сообщений в канал");

				emb.WithDescription(_translator?.MessageWall?.WallName + $" в <#{_translator?.ChannelId}>");

				emb.AddField("Что сделать?", "** **");

				await _session.Args.EditOriginalResponseAsync(new DiscordWebhookBuilder()
					.AddEmbed(emb).AddComponents(syncBtn, linkChnlBtn, linkWallBtn)
					.AddComponents(openWallBtn, remBtn, exitBtn));

				var response = await _session.GetInteraction();

				if (response.CompareButton(linkChnlBtn))
					return new(LinkChannel);

				await _session.Respond(InteractionResponseType.DeferredMessageUpdate);

				if (response.CompareButton(remBtn))
					return new(RemoveTranslator);

				if (response.CompareButton(syncBtn))
					return new(ForceSyncTranslator);

				if (response.CompareButton(linkWallBtn))
					return new(_wallSelector.SelectWall);

				if (response.CompareButton(openWallBtn))
					return new(OpenWall);

				return new(_ret);
			}

			private async Task<NextNetworkInstruction> ForceSyncTranslator(NetworkInstructionArgument arg)
			{
				await _session.Args.EditOriginalResponseAsync(new DiscordWebhookBuilder()
					.WithContent("Работаем..."));
				var result = await _session.Client.Domain.MsgWallCtr.PostMessageUpdate(_translator.ID);


				var changed = await result;
				await _session.Args.EditOriginalResponseAsync(new DiscordWebhookBuilder()
					.WithContent($"Обновлено {changed}"));
				await Task.Delay(TimeSpan.FromSeconds(5));

				return new(ShowOptions);
			}

			private async Task<NextNetworkInstruction> LinkChannel(NetworkInstructionArgument arg)
			{
				await _session.Respond(new DiscordInteractionResponseBuilder().WithContent("Введите id канала"));

				return new(WaitAndRetryLink);
			}

			private async Task<NextNetworkInstruction> WaitAndRetryLink(NetworkInstructionArgument arg)
			{
				var msg = await _session.GetSessionMessage();

				if (!ulong.TryParse(msg.Content, out var id))
				{
					await _session.Args.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Ошибка!"));
					return new(WaitAndRetryLink);
				}

				using var db = _session.DBFactory.CreateMyDbContext();

				_translator.ChannelId = id;

				db.MessageWallTranslators.Update(_translator);
				await db.SaveChangesAsync();

				return new(ShowOptions);
			}

			private async Task<NextNetworkInstruction> RemoveTranslator(NetworkInstructionArgument arg)
			{
				var returnBtn = new DiscordButtonComponent(ButtonStyle.Success, "return", "Назад");
				var removeBtn = new DiscordButtonComponent(ButtonStyle.Danger, "remove", "***Удалить***");

				var emb = new DiscordEmbedBuilder();
				emb.WithDescription($"**ВЫ УВЕРЕНЫ ЧТО ХОТИТЕ УДАЛИТЬ ТРАНСЛЯТОР №{_translator.ID}?**");
				await _session.Args.EditOriginalResponseAsync(new DiscordWebhookBuilder()
					.AddEmbed(emb).AddComponents(returnBtn, removeBtn));

				var response = await _session.GetInteraction();

				await _session.Respond(InteractionResponseType.DeferredMessageUpdate);


				if (!response.CompareButton(removeBtn))
				{
					await _session.Respond(InteractionResponseType.UpdateMessage,
						new DiscordInteractionResponseBuilder()
						.AddComponents(returnBtn.Disable(), removeBtn.Disable()));
				}

				if (response.CompareButton(returnBtn))
					return new(ShowOptions);

				using var db = _session.DBFactory.CreateMyDbContext();

				try
				{
					try
					{
						var channel = await _session.Client.Client.GetChannelAsync(_translator.ChannelId);
						foreach (var id in _translator.Translation)
						{
							try
							{
								var msg = await channel.GetMessageAsync(id);
								await msg.DeleteAsync();
							}
							catch (NotFoundException) { }
						}
					}
					catch (NotFoundException) { }

					db.MessageWallTranslators.Remove(_translator);
					await db.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException) { }

				_translator = null;

				return new(_ret);
			}
			private async Task<NextNetworkInstruction> ChangeWall(NetworkInstructionArgument args)
			{
				var itm = (MessageWall)args.Payload;

				if (itm == null)
				{
					await _session.Respond(InteractionResponseType.DeferredMessageUpdate);
					return new(ShowOptions);
				}

				_translator.MessageWall = itm;

				using var db = await _session.DBFactory.CreateMyDbContextAsync();
				db.MessageWalls.Update(itm);
				db.MessageWallTranslators.Update(_translator);
				await db.SaveChangesAsync();

				return new(ShowOptions);
			}
			private async Task<NextNetworkInstruction> OpenWall(NetworkInstructionArgument args)
			{
				var wall = await _wallSelector.Decorator(_wallSelector.Querryer()
					.Where(x => x == _translator.MessageWall)).FirstAsync();

				return _wallEditor.GetStartingInstruction(wall);
			}
			public NextNetworkInstruction GetStartingInstruction()
			{
				throw new NotImplementedException();
			}
			public NextNetworkInstruction GetStartingInstruction(object payload)
			{
				_translator = (MessageWallTranslator)payload;
				if (_translator != null)
				{
					_wallEditor = new(_session, new(ShowOptions));
					_wallSelector = new(_session, ChangeWall);
				}
				return new(ShowOptions);
			}
		}
		public class Selector : INodeNetwork
		{
			private MessageWallSession _session;
			private InteractiveSelectMenu<MessageWallTranslator> _selectMenu;
			private Node _ret;
			public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;
			public DiscordButtonComponent MkNewButton;
			public DiscordButtonComponent EditButton;
			private readonly MessageWall _wall;
			private class MyQuerrier : IQuerrier<MessageWallTranslator>
			{
				private IPermMessageDbFactory _factory;
				public MyQuerrier(IPermMessageDbFactory factory)
				{
					_factory = factory;
				}
				public IItemDescriptor<MessageWallTranslator> Convert(MessageWallTranslator item) =>
					new Descriptor(item);
				public int GetPages(int perPage)
				{
					using var db = _factory.CreateMyDbContext();

					return (int)Math.Ceiling((double)GetTotalCount() / perPage);
				}
				public IEnumerable<MessageWallTranslator> GetSection(int skip, int take)
				{
					using var db = _factory.CreateMyDbContext();

					var input = db.MessageWallTranslators.OrderBy(x => x.ID).Skip(skip).Take(take);
					return input.Include(x => x.MessageWall).ToArray();
				}
				public int GetTotalCount()
				{
					using var db = _factory.CreateMyDbContext();
					return db.MessageWallTranslators.OrderBy(x => x.ID).Count();
				}
			}
			public Selector(MessageWallSession session, Node ret, MessageWall wall)
			{
				(_wall, _session, _ret) = (wall, session, ret);
				_selectMenu = new InteractiveSelectMenu<MessageWallTranslator>(_session,
					new QueryablePageReturner<MessageWallTranslator>(new MyQuerrier(_session.DBFactory)));
				EditButton = new DiscordButtonComponent(ButtonStyle.Primary, "edit", "Изменить");
				MkNewButton = new DiscordButtonComponent(ButtonStyle.Success, "create", "Создать");
			}
			private async Task<NextNetworkInstruction> CreateNew(NetworkInstructionArgument args)
			{
				using var db = await _session.DBFactory.CreateMyDbContextAsync();
				var line = new MessageWallTranslator();

				if (_wall != null)
				{
					line.MessageWall = _wall;
					db.MessageWalls.Update(_wall);
				}

				db.MessageWallTranslators.Add(line);
				await db.SaveChangesAsync();

				return new(_ret, line);
			}
			public async Task<NextNetworkInstruction> SelectToEdit(NetworkInstructionArgument args)
			{
				var line = (await _selectMenu.EvaluateItem())?.GetCarriedItem();


				return new(_ret, line);
			}

			public NextNetworkInstruction GetStartingInstruction()
			{
				throw new NotImplementedException();
			}

			public NextNetworkInstruction GetStartingInstruction(object payload)
			{
				var resp = payload as InteractiveInteraction;

				if (resp.CompareButton(EditButton))
					return new(SelectToEdit);

				if (resp.CompareButton(MkNewButton))
					return new(CreateNew);

				throw new NotImplementedException();
			}
		}
		private MessageWallSession _session;
		private Editor _editor;
		private Selector _selector;
		public MsgWallPanelWallTranslator(MessageWallSession session)
		{
			_session = session;
			_selector = new(session, Decider, null);
			_editor = new(session, new(_selector.SelectToEdit));
		}
		private async Task<NextNetworkInstruction> EnterMenu(NetworkInstructionArgument args)
		{
			var syncBtn = new DiscordButtonComponent(ButtonStyle.Secondary, "syncall", "Синхронизировать всех");
			var exitBtn = new DiscordButtonComponent(ButtonStyle.Danger, "exit", "Выйти");

			var response = await _session.RespondAndWait(new DiscordInteractionResponseBuilder()
				.WithContent("Добро пожаловать в меню управления транслятора стены строк!")
				.AddComponents(_selector.MkNewButton.Enable(), _selector.EditButton.Enable())
				.AddComponents(syncBtn, exitBtn));

			if (response.CompareButton(exitBtn))
				return new();

			await _session.Respond(new DiscordInteractionResponseBuilder()
				.WithContent("Добро пожаловать в меню управления транслятора стены строк!")
				.AddComponents(_selector.MkNewButton.Disable(), _selector.EditButton.Disable())
				.AddComponents(syncBtn.Disable(), exitBtn.Disable()));


			if (response.CompareButton(syncBtn))
				return new(ForceSyncAll);

			return _selector.GetStartingInstruction(response);
		}
		private async Task<NextNetworkInstruction> ForceSyncAll(NetworkInstructionArgument arg)
		{
			var sent = _session.Args.EditOriginalResponseAsync(new DiscordWebhookBuilder()
				.WithContent($"Работаем..."));

			var list = new List<ulong>();

			using (var db = _session.DBFactory.CreateMyDbContext())
			{
				list = db.MessageWallTranslators.Select(x => x.ID).ToList();
			}

			var changedAll = 0;
			foreach (var id in list)
			{
				changedAll += await await _session.Client.Domain.MsgWallCtr.PostMessageUpdate(id) ?? 0;
			}

			await sent;
			await _session.Args.EditOriginalResponseAsync(new DiscordWebhookBuilder()
				.WithContent($"Всего изменено {changedAll} строк у {list.Count} стен сообщений.")
				.AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "ok", "Ок")));

			await _session.GetInteraction();

			return new(EnterMenu);
		}
		private async Task<NextNetworkInstruction> Decider(NetworkInstructionArgument args)
		{
			var itm = (MessageWallTranslator)args.Payload;

			if (itm == null)
				return new(EnterMenu);

			return _editor.GetStartingInstruction(itm);
		}
		public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;
		public NextNetworkInstruction GetStartingInstruction() => new(EnterMenu);
		public NextNetworkInstruction GetStartingInstruction(object payload) => GetStartingInstruction();
	}
}