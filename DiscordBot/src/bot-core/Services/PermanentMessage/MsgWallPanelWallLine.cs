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
        private class MsgWallDescriptor : IItemDescriptor<MessageWallLine>
        {
            private readonly MessageWallLine _wallLine;
            public MsgWallDescriptor(MessageWallLine wallLine) => _wallLine = wallLine;
            private int _lid;
            private int _gid;
            public string GetButtonId() => $"MessageWallLine{_lid}";

            public string GetButtonName() => $"Стена {_lid} ID:{_wallLine.ID}";

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
        private MessageWallSession _session;
        private NextNetworkInstruction _ret;
        private MessageWallLine _line;
        private InteractiveSelectMenu<MessageWallLine> _selectMenu;
        public MsgWallPanelWallLine(MessageWallSession session, NextNetworkInstruction ret)
        {
            _session = session;
            _ret = ret;
            _selectMenu = new InteractiveSelectMenu<MessageWallLine>(_session,
                new QueryablePageReturner<MessageWallLine>(Querryer, x => new MsgWallDescriptor(x)));
        }
        private async Task<NextNetworkInstruction> EnterMenu(NetworkInstructionArgument args)
        {

            var exitBtn = new DiscordButtonComponent(ButtonStyle.Danger, "exit", "Выйти");
            var editBtn = new DiscordButtonComponent(ButtonStyle.Primary, "edit", "Изменить");
            var mkNewBtn = new DiscordButtonComponent(ButtonStyle.Primary, "create", "Создать");
            var createTestBtn = new DiscordButtonComponent(ButtonStyle.Secondary, "createtest", "Создать тест-датасет");


            await _session.Respond(InteractionResponseType.UpdateMessage,
                new DiscordInteractionResponseBuilder()
                .WithContent("Добро пожаловать в меню управления строками!")
                .AddComponents(mkNewBtn, editBtn, exitBtn));

            var response = await _session.GetInteraction();

            if (response.CompareButton(exitBtn))
                return new();

            await _session.Respond(InteractionResponseType.UpdateMessage,
            new DiscordInteractionResponseBuilder()
                .WithContent("Добро пожаловать в меню управления строками!")
                .AddComponents(mkNewBtn.Disable(), editBtn.Disable(), exitBtn.Disable()));

            if (response.CompareButton(mkNewBtn)) return new(CreateNew);
            if (response.CompareButton(createTestBtn)) return new(CreateTestData);
            if (response.CompareButton(editBtn)) return new(SelectToEdit);

            return new();
        }
        private async Task<NextNetworkInstruction> CreateTestData(NetworkInstructionArgument args)
        {
            using var db = await _session.DBFactory.CreateMyDbContextAsync();
            for (var i = 0; i < 450; i++)
            {
                for (var g = 0; g < 1000; g++)
                    db.MessageWallLines.Add(new($"{(i * 1000) + g}"));

                await db.SaveChangesAsync();
            }

            return new(EnterMenu);
        }
        private async Task<NextNetworkInstruction> CreateNew(NetworkInstructionArgument args)
        {
            using var db = await _session.DBFactory.CreateMyDbContextAsync();
            _line = new();
            db.MessageWallLines.Add(_line);
            await db.SaveChangesAsync();

            _lilGoBackInstr = EnterMenu;
            return new(ShowOptions);
        }
        private IQueryable<MessageWallLine> Querryer()
        {
            using var db = _session.DBFactory.CreateMyDbContext();

            return db.MessageWallLines;
        }
        private async Task<NextNetworkInstruction> SelectToEdit(NetworkInstructionArgument args)
        {

            _line = (await _selectMenu.EvaluateItem())?.GetCarriedItem();

            _lilGoBackInstr = SelectToEdit;
            return new(_line == null ? EnterMenu : ShowOptions);
        }
        private Node _lilGoBackInstr;
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

            if (_lilGoBackInstr == SelectToEdit || !response.CompareButton(exitBtn))
                await _session.Respond(InteractionResponseType.DeferredMessageUpdate);

            if (response.CompareButton(remBtn))
                return new(RemoveLine);

            if (response.CompareButton(editContentBtn))
                return new(EditContent);

            if (response.CompareButton(changeWallBtn))
                return new(ChangeWall);

            if (response.CompareButton(openWallBtn))
                return new(OpenWall);

            return new(_lilGoBackInstr);
        }

        private async Task<NextNetworkInstruction> RemoveLine(NetworkInstructionArgument args)
        {

            var returnBtn = new DiscordButtonComponent(ButtonStyle.Success, "return", "Назад");
            var removeBtn = new DiscordButtonComponent(ButtonStyle.Danger, "remove", "***Удалить***");
            var emb = new DiscordEmbedBuilder();

            emb.WithDescription($"ВЫ УВЕРЕНЫ ЧТО ХОТИТЕ УДАЛИТЬ {_line.ID}?");
            await _session.Args.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(emb).AddComponents(returnBtn, removeBtn));

            var response = await _session.GetInteraction();

            if (_lilGoBackInstr == SelectToEdit || !response.CompareButton(removeBtn))
            {
                await _session.Respond(InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder()
                    .AddComponents(returnBtn.Disable(), removeBtn.Disable()));
            }

            if (response.CompareButton(returnBtn))
                return new(ShowOptions);

            using var db = await _session.DBFactory.CreateMyDbContextAsync();

            db.MessageWallLines.Update(_line);
            await db.SaveChangesAsync();

            db.MessageWallLines.Remove(_line);
            await db.SaveChangesAsync();

            _line = null;


            return new(_lilGoBackInstr);

        }
        private async Task<NextNetworkInstruction> EditContent(NetworkInstructionArgument args)
        {
            using var db = await _session.DBFactory.CreateMyDbContextAsync();
            db.MessageWallLines.Update(_line);

            await _session.Args.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                .WithContent("Напишите содержимое стены сообщением"));

            var msg = await _session.GetSessionMessage();

            _line.SetLine(msg.Content);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                db.MessageWallLines.Add(_line);
                await db.SaveChangesAsync();
            }

            return new(ShowOptions);
        }
        private async Task<NextNetworkInstruction> ChangeWall(NetworkInstructionArgument args)
        {

            return new(ShowOptions);
        }
        private async Task<NextNetworkInstruction> OpenWall(NetworkInstructionArgument args)
        {

            return new(ShowOptions);
        }

        public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;
        public NextNetworkInstruction GetStartingInstruction() => new(EnterMenu);
        public NextNetworkInstruction GetStartingInstruction(object payload) => GetStartingInstruction();
    }
}