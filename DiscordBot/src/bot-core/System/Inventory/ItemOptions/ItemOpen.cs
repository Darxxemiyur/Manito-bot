using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.EventArgs;
using Manito.Discord.Client;
using Name.Bayfaderix.Darxxemiyur.Common;
using Manito.Discord.Chat.DialogueNet;
using Name.Bayfaderix.Darxxemiyur.Node.Network;

namespace Manito.Discord.Inventory
{
    public class ItemSelect : IDialogueNet
    {
        private IItem _item;
        private InventorySession _session;

        public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;

        public ItemSelect(IItem item, InventorySession session)
        {
            _item = item;
            _session = session;
        }

        private async Task<NextNetworkInstruction> ShowOptions(NetworkInstructionArgument args)
        {
            throw new NotImplementedException();
        }
        public NextNetworkInstruction GetStartingInstruction(object payload) => GetStartingInstruction();
        public NextNetworkInstruction GetStartingInstruction() => new(ShowOptions, NextNetworkActions.Continue);

    }
}