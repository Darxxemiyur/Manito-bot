using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using Manito.Discord.Chat.DialogueNet;
using Manito.Discord.Client;
using Microsoft.EntityFrameworkCore;
using Name.Bayfaderix.Darxxemiyur.Node.Network;

namespace Manito.Discord.PermanentMessage
{
    public class MsgWallPanelWall : INodeNetwork
    {
        private class Descriptor : IItemDescriptor<MessageWall>
        {
            private readonly MessageWall _wall;
            public Descriptor(MessageWall wall) => _wall = wall;
            private int _lid;
            private int _gid;
            public string GetButtonId() => $"MessageWall{_lid}";

            public string GetButtonName() => $"Стена {_lid} ID:{_wall.ID}";

            public MessageWall GetCarriedItem() => _wall;

            public string GetFieldBody() => throw new NotImplementedException();

            public string GetFieldName() => throw new NotImplementedException();

            public int GetGlobalDisplayOrder() => _gid;

            public int GetLocalDisplayOrder() => _lid;

            public bool HasButton() => true;

            public bool HasField() => false;

            public IItemDescriptor<MessageWall> SetGlobalDisplayedOrder(int i)
            {
                _gid = i;
                return this;
            }

            public IItemDescriptor<MessageWall> SetLocalDisplayedOrder(int i)
            {
                _lid = i;
                return this;
            }
        }
public class 
        private MessageWallSession _session;
        private MessageWall _wall;
        private InteractiveSelectMenu<MessageWall> _wallSelectMenu;
        private InteractiveSelectMenu<MessageWallLine> _childSelectMenu;
        public MsgWallPanelWall(MessageWallSession session)
        {
            _session = session;
            _wallSelectMenu = new InteractiveSelectMenu<MessageWall>(session,
                new QueryablePageReturner<MessageWall>(Querryer, x => new Descriptor(x)));
            _childSelectMenu = new InteractiveSelectMenu<MessageWallLine>(session,
                new QueryablePageReturner<MessageWallLine>(ChildQuerryer,
                 x => new MsgWallPanelWallLine.Descriptor(x)));
        }
        private IQueryable<MessageWall> Querryer()
        {
            using var db = _session.DBFactory.CreateMyDbContext();

            return db.MessageWalls;
        }
        private IQueryable<MessageWallLine> ChildQuerryer()
        {
            using var db = _session.DBFactory.CreateMyDbContext();

            return db.MessageWallLines.Where(x => x.MessageWall == _wall);
        }
        private Node _lilGoBackInstr;
        private async Task<NextNetworkInstruction> EnterMenu(NetworkInstructionArgument args)
        {
            var createBtn = new DiscordButtonComponent(ButtonStyle.Success, "create", "Создать");
            var selectBtn = new DiscordButtonComponent(ButtonStyle.Primary, "select", "Выбрать");
            var exitBtn = new DiscordButtonComponent(ButtonStyle.Danger, "exit", "Выйти");


            var response = await _session.RespondAndWait(new DiscordInteractionResponseBuilder()
                .WithContent("Добро пожаловать в меню управления стены строк!")
                .AddComponents(createBtn, selectBtn, exitBtn));


            if (response.CompareButton(exitBtn))
                return new();

            await _session.Respond(InteractionResponseType.UpdateMessage,
            new DiscordInteractionResponseBuilder()
                .WithContent("Добро пожаловать в меню управления стены строк!")
                .AddComponents(createBtn.Disable(), selectBtn.Disable(), exitBtn.Disable()));


            Node next = null;

            if (response.CompareButton(createBtn))
                next = CreateWall;
            if (response.CompareButton(selectBtn))
                next = SelectWall;

            return new(next);
        }

        private async Task<NextNetworkInstruction> CreateWall(NetworkInstructionArgument args)
        {
            using var db = await _session.DBFactory.CreateMyDbContextAsync();
            _wall = new();
            db.MessageWalls.Add(_wall);
            await db.SaveChangesAsync();

            _lilGoBackInstr = EnterMenu;
            return new(ActionsChoose);
        }

        private async Task<NextNetworkInstruction> SelectWall(NetworkInstructionArgument args)
        {
            _wall = (await _wallSelectMenu.EvaluateItem())?.GetCarriedItem();

            _lilGoBackInstr = SelectWall;
            return new(_wall == null ? EnterMenu : ActionsChoose);
        }

        private async Task<NextNetworkInstruction> SelectWallChildren(NetworkInstructionArgument args)
        {
            var child = (await _childSelectMenu.EvaluateItem())?.GetCarriedItem();

            if (child == null)
            {
                await _session.Respond(InteractionResponseType.DeferredMessageUpdate);
                return new(ActionsChoose);
            }

            var diag = new MsgWallPanelWallLine(_session);

            await NetworkCommon.RunNetwork(diag, child);

            return new(ActionsChoose);
        }

        private async Task<NextNetworkInstruction> ActionsChoose(NetworkInstructionArgument args)
        {
            var renameBtn = new DiscordButtonComponent(ButtonStyle.Secondary, "rename", "Назвать");
            var listBtn = new DiscordButtonComponent(ButtonStyle.Primary, "list", "Показать строки");
            var remBtn = new DiscordButtonComponent(ButtonStyle.Danger, "remove", "Удалить");
            var exitBtn = new DiscordButtonComponent(ButtonStyle.Danger, "exit", "Выйти");
            var emb = new DiscordEmbedBuilder();

            emb.WithAuthor("Стена сообщений");
            emb.WithDescription(_wall.WallName);
            emb.AddField("Что сделать?", "** **");

            await _session.Args.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(emb).AddComponents(renameBtn, listBtn)
                .AddComponents(remBtn, exitBtn));

            var response = await _session.GetInteraction();

            if (_lilGoBackInstr == SelectWall || !response.CompareButton(exitBtn))
                await _session.Respond(InteractionResponseType.DeferredMessageUpdate);

            if (response.CompareButton(renameBtn))
                return new(RenameWall);

            if (response.CompareButton(listBtn))
                return new(SelectWallChildren);

            if (response.CompareButton(remBtn))
                return new(RemoveWall);

            return new(_lilGoBackInstr);
        }
        private async Task<NextNetworkInstruction> RenameWall(NetworkInstructionArgument args)
        {
            using var db = await _session.DBFactory.CreateMyDbContextAsync();
            db.MessageWalls.Update(_wall);

            await _session.Args.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                .WithContent("Напишите желаемое название стены сообщений"));

            var msg = await _session.GetSessionMessage();

            _wall.SetName(msg.Content);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await _session.Args.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                    .WithContent("Стена была удалена во время редактирования."));
            }

            return new(ActionsChoose);
        }

        private async Task<NextNetworkInstruction> RemoveWall(NetworkInstructionArgument args)
        {

            var returnBtn = new DiscordButtonComponent(ButtonStyle.Success, "return", "Назад");
            var removeBtn = new DiscordButtonComponent(ButtonStyle.Danger, "remove", "***Удалить***");

            var emb = new DiscordEmbedBuilder();
            emb.WithDescription($"**ВЫ УВЕРЕНЫ ЧТО ХОТИТЕ УДАЛИТЬ {_wall.ID}?**");
            await _session.Args.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(emb).AddComponents(returnBtn, removeBtn));

            var response = await _session.GetInteraction();

            if (_lilGoBackInstr == SelectWall || !response.CompareButton(removeBtn))
            {
                await _session.Respond(InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder()
                    .AddComponents(returnBtn.Disable(), removeBtn.Disable()));
            }

            if (response.CompareButton(returnBtn))
                return new(ActionsChoose);

            using var db = await _session.DBFactory.CreateMyDbContextAsync();
            try
            {
                db.MessageWalls.Update(_wall);
                await db.ImplementedContext.Entry(_wall)
                .Collection(x => x.Msgs).Query().LoadAsync();
                _wall.Msgs.Clear();

                foreach (var translator in db.MessageWallTranslators
                    .Where(x => x.MessageWall == _wall).ToList())
                {
                    translator.MessageWall = null;
                }

                db.MessageWalls.Remove(_wall);
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) { }

            _wall = null;

            return new(_lilGoBackInstr);
        }

        public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;
        public NextNetworkInstruction GetStartingInstruction() => new(EnterMenu);
        public NextNetworkInstruction GetStartingInstruction(object payload) => GetStartingInstruction();
    }
}