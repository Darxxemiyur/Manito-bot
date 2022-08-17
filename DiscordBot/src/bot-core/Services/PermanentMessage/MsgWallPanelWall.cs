using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using Manito.Discord.Chat.DialogueNet;
using Name.Bayfaderix.Darxxemiyur.Node.Network;

namespace Manito.Discord.PermanentMessage
{
    public class MsgWallPanelWall : INodeNetwork
    {
        private MessageWallSession _session;
        private NextNetworkInstruction _ret;
        public MsgWallPanelWall(MessageWallSession session, NextNetworkInstruction ret)
        {
            _session = session;
            _ret = ret;
        }
        public async Task<NextNetworkInstruction> EnterMenu(NetworkInstructionArgument args)
        {
            using var db = await _session.DBFactory.CreateMyDbContextAsync();


            var exitBtn = new DiscordButtonComponent(ButtonStyle.Danger, "exit", "Выйти");
            var response = await _session.RespondAndWait(new DiscordInteractionResponseBuilder()
                .WithContent("Добро пожаловать в меню управления стены строк!")
                .AddComponents(exitBtn));

            return new();
        }

        public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;
        public NextNetworkInstruction GetStartingInstruction() => new(EnterMenu);
        public NextNetworkInstruction GetStartingInstruction(object payload) => GetStartingInstruction();
    }
}