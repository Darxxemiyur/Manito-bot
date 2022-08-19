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
    public class MsgWallPanelWallTranslator : INodeNetwork
    {
        private class MsgWallWallTranslatorDescriptor : IItemDescriptor<MessageWallLine>
        {
            private readonly MessageWallLine _wallLine;
            public MsgWallWallTranslatorDescriptor(MessageWallLine wallLine) => _wallLine = wallLine;
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
            var exitBtn = new DiscordButtonComponent(ButtonStyle.Danger, "exit", "Выйти");

            var response = await _session.RespondAndWait(new DiscordInteractionResponseBuilder()
                .WithContent("Добро пожаловать в меню управления транслятора стены строк!")
                .AddComponents(createBtn, selectBtn, exitBtn));

            throw new NotImplementedException();
        }
        private async Task<NextNetworkInstruction> CreateNewWall(NetworkInstructionArgument arg)
        {
            using var db = await _session.DBFactory.CreateMyDbContextAsync();



            throw new Exception();
        }
        private async Task<NextNetworkInstruction> SelectExisting(NetworkInstructionArgument arg)
        {
            throw new Exception();
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