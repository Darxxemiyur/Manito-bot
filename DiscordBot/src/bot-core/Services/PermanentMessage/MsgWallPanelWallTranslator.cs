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

namespace Manito.Discord.PermanentMessage
{
    public class MsgWallPanelWallTranslator : INodeNetwork
    {
        private class Descriptor : IItemDescriptor<MessageWallLine>
        {
            private readonly MessageWallLine _wallLine;
            public Descriptor(MessageWallLine wallLine) => _wallLine = wallLine;
            private int _lid;
            private int _gid;
            public string GetButtonId() => $"MessageWallLine{_lid}";

            public string GetButtonName() => $"Стена {_lid}";

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
                 "Открыть стену строки", _translator.MessageWall == null);
                var remBtn = new DiscordButtonComponent(ButtonStyle.Danger, "remove", "Удалить");
                var exitBtn = new DiscordButtonComponent(ButtonStyle.Danger, "exit", "Выйти");

                var emb = new DiscordEmbedBuilder();

                emb.WithAuthor("Транслятор стены сообщений в канал");

                emb.WithDescription(_translator.MessageWall.WallName + $" в <#{_translator.ChannelId}>");

                emb.AddField("Что сделать?", "** **");

                await _session.Args.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(emb).AddComponents(syncBtn, linkChnlBtn, linkWallBtn)
                    .AddComponents(openWallBtn, remBtn, exitBtn));

                var response = await _session.GetInteraction();

                await _session.Respond(InteractionResponseType.DeferredMessageUpdate);

                if (response.CompareButton(remBtn))
                    return new(RemoveTranslator);

                if (response.CompareButton(syncBtn))
                    return new(ForceSyncTranslator);

                if (response.CompareButton(linkChnlBtn))
                    return new(LinkChannel);

                if (response.CompareButton(linkWallBtn))
                    return new(_wallSelector.SelectWall);

                if (response.CompareButton(openWallBtn))
                    return new(OpenWall);

                return new(_ret);
            }

            private async Task<NextNetworkInstruction> ForceSyncTranslator(NetworkInstructionArgument arg)
            {
                var result = await await _session.Client.Domain.MsgWallCtr.PostMessageUpdate(_translator.ID);



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

                if (!ulong.TryParse(msg.Content, out ulong id))
                {
                    await _session.Args.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Ошибка!"));
                    return new(WaitAndRetryLink);
                }

                _translator.ChannelId = id;

                return new(ShowOptions);
            }

            private async Task<NextNetworkInstruction> LinkWall(NetworkInstructionArgument arg)
            {



                return new(ShowOptions);
            }

            private async Task<NextNetworkInstruction> RemoveTranslator(NetworkInstructionArgument arg)
            {
                await _session.Respond(new DiscordInteractionResponseBuilder()
                    .WithContent("Пока-что не работет."));

                await Task.Delay(TimeSpan.FromSeconds(4));

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
        private NextNetworkInstruction _ret;
        public MsgWallPanelWallTranslator(MessageWallSession session, NextNetworkInstruction ret)
        {
            _session = session;
            _ret = ret;
        }
        private async Task<NextNetworkInstruction> EnterMenu(NetworkInstructionArgument args)
        {

            var createBtn = new DiscordButtonComponent(ButtonStyle.Success, "create", "Создать");
            var selectBtn = new DiscordButtonComponent(ButtonStyle.Primary, "select", "Выбрать");
            var syncBtn = new DiscordButtonComponent(ButtonStyle.Secondary, "syncall", "Синхронизировать всех");
            var exitBtn = new DiscordButtonComponent(ButtonStyle.Danger, "exit", "Выйти");

            var response = await _session.RespondAndWait(new DiscordInteractionResponseBuilder()
                .WithContent("Добро пожаловать в меню управления транслятора стены строк!")
                .AddComponents(createBtn, selectBtn).AddComponents(syncBtn, exitBtn));

            throw new NotImplementedException();
        }
        private async Task<NextNetworkInstruction> CreateNewTranslator(NetworkInstructionArgument arg)
        {
            using var db = await _session.DBFactory.CreateMyDbContextAsync();



            throw new Exception();
        }
        private async Task<NextNetworkInstruction> SelectExistingTranslator(NetworkInstructionArgument arg)
        {
            throw new Exception();
        }
        private async Task<NextNetworkInstruction> ForceSyncAll(NextNetworkInstruction arg)
        {
            var list = new List<ulong>();

            using (var db = _session.DBFactory.CreateMyDbContext())
            {
                list = db.MessageWallTranslators.Select(x => x.ID).ToList();
            }

            foreach (var id in list)
            {
                await _session.Client.Domain.MsgWallCtr.PostMessageUpdate(id);
            }

            return new();
        }
        private async Task<NextNetworkInstruction> SelectActions(NetworkInstructionArgument arg)
        {
            throw new Exception();
        }

        public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;
        public NextNetworkInstruction GetStartingInstruction() => new(EnterMenu);
        public NextNetworkInstruction GetStartingInstruction(object payload) => GetStartingInstruction();
    }
}