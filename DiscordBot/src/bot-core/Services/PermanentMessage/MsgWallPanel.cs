using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using Name.Bayfaderix.Darxxemiyur.Common;
using System.Collections.Generic;
using Manito.Discord.Economy;
using DSharpPlus.EventArgs;
using Manito.Discord.Chat.DialogueNet;
using Manito.Discord.Inventory;
using Name.Bayfaderix.Darxxemiyur.Node.Network;
using System.Diagnostics;

namespace Manito.Discord.PermanentMessage
{
    /// <summary>
    /// MessageWall Service Menu dialogue
    /// </summary>
    public class MsgWallPanel : IDialogueNet
    {
        private MessageWallSession _session;
        public DiscordButtonComponent CreateTestButton;
        public MsgWallPanel(MessageWallSession session)
        {
            _session = session;
            CreateTestButton = new DiscordButtonComponent(ButtonStyle.Secondary, "createtest", "Создать тест-датасет");
        }

        public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;

        public NextNetworkInstruction GetStartingInstruction()
        {
            return new(SelectWhatToDo);
        }

        public NextNetworkInstruction GetStartingInstruction(object payload)
        {
            return GetStartingInstruction();
        }
        private async Task<NextNetworkInstruction> CreateTestData(NetworkInstructionArgument args)
        {
            using var db = await _session.DBFactory.CreateMyDbContextAsync();
            for (var i = 0; i < 4500; i++)
            {
                var newItmChild = new List<MessageWallLine>();
                for (var g = 0; g < 100; g++)
                {
                    newItmChild.Add(new($"{(i * 1000) + g}"));
                }
                var newItm = new MessageWall($"w-{i * 1000}")
                {
                    Msgs = newItmChild
                };

                db.MessageWallLines.AddRange(newItmChild);
                db.MessageWalls.Add(newItm);

                await db.SaveChangesAsync();
            }

            return new(SelectWhatToDo);
        }
        private async Task<NextNetworkInstruction> SelectWhatToDo(NetworkInstructionArgument arg)
        {
            var wallLine = new DiscordButtonComponent(ButtonStyle.Primary, "wallLine", "Единицу");
            var wall = new DiscordButtonComponent(ButtonStyle.Primary, "wall", "Набор единиц");
            var wallTranslator = new DiscordButtonComponent(ButtonStyle.Primary, "wallTranslator", "Переводчик");

            var exitBtn = new DiscordButtonComponent(ButtonStyle.Danger, "exit", "Выйти");

            var response = await _session.RespondAndWait(new DiscordInteractionResponseBuilder()
                .WithContent("Выбор изменения информации")
                .AddComponents(wallLine, wall, wallTranslator, CreateTestButton)
                .AddComponents(exitBtn));

            var intr = _session.RespondAndWait(new DiscordInteractionResponseBuilder()
                .WithContent("Выбор изменения информации")
                .AddComponents(wallLine.Disable(), wall.Disable(), wallTranslator.Disable(), CreateTestButton.Disable())
                .AddComponents(exitBtn.Disable()));



            if (response.CompareButton(wallLine))
            {
                var next = new MsgWallPanelWallLine(_session);
                await NetworkCommon.RunNetwork(next);
            }

            if (response.CompareButton(wall))
            {
                var next = new MsgWallPanelWall(_session);
                await NetworkCommon.RunNetwork(next);
            }

            if (response.CompareButton(CreateTestButton))
                return new(CreateTestData);

            if (response.CompareButton(wallTranslator))
            {
                var next = new MsgWallPanelWallTranslator(_session, new(SelectWhatToDo));
                await NetworkCommon.RunNetwork(next);
            }

            if (response.CompareButton(exitBtn))
                return new();

            return new(SelectWhatToDo);
        }
    }
}