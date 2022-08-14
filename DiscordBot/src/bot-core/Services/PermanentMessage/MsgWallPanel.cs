using System;
using System.Threading.Tasks;
using System.Linq;

using DSharpPlus;
using DSharpPlus.Entities;

using Manito.Discord.Client;
using Name.Bayfaderix.Darxxemiyur.Common;
using System.Collections.Generic;
using Manito.Discord.Economy;
using DSharpPlus.EventArgs;
using Manito.Discord.Chat.DialogueNet;
using Manito.Discord.Inventory;
using Name.Bayfaderix.Darxxemiyur.Node.Network;

namespace Manito.Discord.PermanentMessage
{

    public class MsgWallPanel : IDialogueNet
    {
        private MessageWallSession _session;
        public MsgWallPanel(MessageWallSession session)
        {
            _session = session;
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
        private async Task<NextNetworkInstruction> SelectWhatToDo(NetworkInstructionArguments arg)
        {
            try
            {
                await _session.RespondLater();
                await Task.Delay(5000);

                await _session.Respond(new DiscordInteractionResponseBuilder().WithContent("Eurika!"));

                await Task.Delay(2000);

                await _session.RespondLater();
                await Task.Delay(5000);

                await _session.Respond(new DiscordInteractionResponseBuilder().WithContent("Eurika!"));

                await _session.QuitSession();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
            return new();
        }
    }
}