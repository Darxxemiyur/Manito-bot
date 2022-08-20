using System;
using System.Threading.Tasks;
using System.Linq;

using DSharpPlus;
using DSharpPlus.Entities;

using Manito.Discord.Client;
using Manito.Discord.Chat.DialogueNet;
using Name.Bayfaderix.Darxxemiyur.Node.Network;
using Microsoft.EntityFrameworkCore;

namespace Manito.Discord.PermanentMessage
{
    public class MsgWallPanelWallLine : INodeNetwork
    {
        public class Descriptor : IItemDescriptor<MessageWallLine>
        {
            private readonly MessageWallLine _wallLine;
            public Descriptor(MessageWallLine wallLine) => _wallLine = wallLine;
            private int _lid;
            private int _gid;
            public string GetButtonId() => $"MessageWallLine{_lid}";
            public string GetButtonName()
            {
                var idStr = $" ID:{_wallLine.ID}";
                var wallName = $"{_wallLine.MessageWall?.WallName ?? ""}".Trim();

                return (string.Concat(wallName.Take(80 - idStr.Length)) + idStr).Trim();
            }
            public MessageWallLine GetCarriedItem() => _wallLine;
            public string GetFieldBody() => throw new NotImplementedException();
            public string GetFieldName() => throw new NotImplementedException();
            public int GetGlobalDisplayOrder() => _gid;
            public int GetLocalDisplayOrder() => _lid;
            public bool HasButton() => true;
            public bool HasField() => false;
            public IItemDescriptor<MessageWallLine> SetGlobalDisplayedOrder(int i)
            {
                _gid = i;
                return this;
            }

            public IItemDescriptor<MessageWallLine> SetLocalDisplayedOrder(int i)
            {
                _lid = i;
                return this;
            }
        }
        public class Editor : INodeNetwork
        {
            private MessageWallSession _session;
            private MessageWallLine _line;
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
                var editContentBtn = new DiscordButtonComponent(ButtonStyle.Primary,
                 "editc", "Редактировать содержимое");
                var changeWallBtn = new DiscordButtonComponent(ButtonStyle.Primary, "editw", "Привязать к стене");

                var openWallBtn = new DiscordButtonComponent(ButtonStyle.Primary, "select",
                 "Открыть стену строки", _line.MessageWall == null);
                var remBtn = new DiscordButtonComponent(ButtonStyle.Danger, "remove", "Удалить");
                var exitBtn = new DiscordButtonComponent(ButtonStyle.Danger, "exit", "Выйти");
                var emb = new DiscordEmbedBuilder();

                emb.WithAuthor("Стена сообщения");

                emb.WithDescription(_line.WallLine);

                emb.AddField("Что сделать?", "** **");

                await _session.Args.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(emb).AddComponents(editContentBtn, changeWallBtn)
                    .AddComponents(openWallBtn, remBtn, exitBtn));

                var response = await _session.GetInteraction();

                await _session.Respond(InteractionResponseType.DeferredMessageUpdate);

                if (response.CompareButton(remBtn))
                    return new(RemoveLine);

                if (response.CompareButton(editContentBtn))
                    return new(EditContent);

                if (response.CompareButton(changeWallBtn))
                    return new(_wallSelector.SelectWall);

                if (response.CompareButton(openWallBtn))
                    return new(OpenWall);

                return new(_ret);
            }
            private async Task<NextNetworkInstruction> RemoveLine(NetworkInstructionArgument args)
            {
                var returnBtn = new DiscordButtonComponent(ButtonStyle.Success, "return", "Назад");
                var removeBtn = new DiscordButtonComponent(ButtonStyle.Danger, "remove", "***Удалить***");

                var emb = new DiscordEmbedBuilder();
                emb.WithDescription($"**ВЫ УВЕРЕНЫ ЧТО ХОТИТЕ УДАЛИТЬ {_line.ID}?**");
                await _session.Args.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(emb).AddComponents(returnBtn, removeBtn));

                var response = await _session.GetInteraction();

                if (!response.CompareButton(removeBtn))
                {
                    await _session.Respond(InteractionResponseType.UpdateMessage,
                        new DiscordInteractionResponseBuilder()
                        .AddComponents(returnBtn.Disable(), removeBtn.Disable()));
                }

                if (response.CompareButton(returnBtn))
                    return new(ShowOptions);

                using var db = await _session.DBFactory.CreateMyDbContextAsync();
                try
                {
                    db.MessageWallLines.Remove(_line);
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) { }

                _line = null;

                await _session.Respond(InteractionResponseType.DeferredMessageUpdate);

                return new(_ret);
            }
            private async Task<NextNetworkInstruction> EditContent(NetworkInstructionArgument args)
            {
                await _session.Args.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                    .WithContent("Напишите содержимое стены сообщением"));

                var msg = await _session.GetSessionMessage();

                using var db = await _session.DBFactory.CreateMyDbContextAsync();
                _line.SetLine(msg.Content);
                db.MessageWallLines.Update(_line);

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    await _session.Args.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                        .WithContent("Строка была удалена во время редактирования."));
                }

                return new(ShowOptions);
            }
            private async Task<NextNetworkInstruction> ChangeWall(NetworkInstructionArgument args)
            {
                var itm = (MessageWall)args.Payload;

                if (itm == null)
                {
                    await _session.Respond(InteractionResponseType.DeferredMessageUpdate);
                    return new(ShowOptions);
                }

                _line.MessageWall = itm;

                using var db = await _session.DBFactory.CreateMyDbContextAsync();
                db.MessageWalls.Update(itm);
                db.MessageWallLines.Update(_line);
                await db.SaveChangesAsync();

                return new(ShowOptions);
            }
            private async Task<NextNetworkInstruction> OpenWall(NetworkInstructionArgument args)
            {
                var wall = await _wallSelector.Decorator(_wallSelector.Querryer()
                    .Where(x => x == _line.MessageWall)).FirstAsync();

                return _wallEditor.GetStartingInstruction(wall);
            }
            public NextNetworkInstruction GetStartingInstruction()
            {
                throw new NotImplementedException();
            }
            public NextNetworkInstruction GetStartingInstruction(object payload)
            {
                _line = (MessageWallLine)payload;
                if (_line != null)
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
            private InteractiveSelectMenu<MessageWallLine> _selectMenu;
            private Node _ret;
            public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;
            public DiscordButtonComponent MkNewButton;
            public DiscordButtonComponent EditButton;
            private readonly MessageWall _wall;
            public Selector(MessageWallSession session, Node ret, MessageWall wall)
            {
                (_wall, _session, _ret) = (wall, session, ret);
                _selectMenu = new InteractiveSelectMenu<MessageWallLine>(_session,
                    new QueryablePageReturner<MessageWallLine>(Querryer, Decorator, x => new Descriptor(x)));
                EditButton = new DiscordButtonComponent(ButtonStyle.Primary, "edit", "Изменить");
                MkNewButton = new DiscordButtonComponent(ButtonStyle.Primary, "create", "Создать");
            }
            private IQueryable<MessageWallLine> Querryer()
            {
                using var db = _session.DBFactory.CreateMyDbContext();

                return db.MessageWallLines.Where(x => x.MessageWall == _wall).OrderBy(x => x.ID);
            }
            private IQueryable<MessageWallLine> Decorator(IQueryable<MessageWallLine> input)
            {
                return input.Include(x => x.MessageWall);
            }
            private async Task<NextNetworkInstruction> CreateNew(NetworkInstructionArgument args)
            {
                using var db = await _session.DBFactory.CreateMyDbContextAsync();
                var line = new MessageWallLine();

                if (_wall != null)
                {
                    line.MessageWall = _wall;
                    db.MessageWalls.Update(_wall);
                }

                db.MessageWallLines.Add(line);
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
        public MsgWallPanelWallLine(MessageWallSession session)
        {
            _selector = new(session, Decider, null);
            _editor = new(session, new(_selector.SelectToEdit));
            _session = session;
        }
        private async Task<NextNetworkInstruction> EnterMenu(NetworkInstructionArgument args)
        {
            var exitBtn = new DiscordButtonComponent(ButtonStyle.Danger, "exit", "Выйти");

            await _session.Respond(InteractionResponseType.UpdateMessage,
                new DiscordInteractionResponseBuilder()
                .WithContent("Добро пожаловать в меню управления строками!")
                .AddComponents(_selector.MkNewButton.Enable(), _selector.EditButton.Enable(), exitBtn));

            var response = await _session.GetInteraction();

            if (response.CompareButton(exitBtn))
                return new();

            await _session.Respond(InteractionResponseType.UpdateMessage,
                new DiscordInteractionResponseBuilder()
                .WithContent("Добро пожаловать в меню управления строками!")
                .AddComponents(_selector.MkNewButton.Disable(),
                 _selector.EditButton.Disable(), exitBtn.Disable()));

            return _selector.GetStartingInstruction(response);
        }
        private async Task<NextNetworkInstruction> Decider(NetworkInstructionArgument args)
        {
            var itm = (MessageWallLine)args.Payload;

            if (itm == null)
                return new(EnterMenu);

            return _editor.GetStartingInstruction(itm);
        }
        public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;
        public NextNetworkInstruction GetStartingInstruction() => new(EnterMenu);
        public NextNetworkInstruction GetStartingInstruction(object payload)
         => GetStartingInstruction();
    }
}