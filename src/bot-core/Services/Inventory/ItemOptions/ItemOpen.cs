using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using Manito.Discord.Client;
using Manito.Discord.Chat.DialogueNet;

namespace Manito.Discord.Inventory
{
    public class ItemOpen : IDialogueNet
    {
        private IItem _item;
        private InventorySession _session;
        public ItemOpen(IItem item, InventorySession session)
        {
            _item = item;
            _session = session;
        }

        private async Task<NextNetInstruction> ShowOptions(InstructionArguments args)
        {
            throw new NotImplementedException();
        }
        public NextNetInstruction GetStartingInstruction() => new(ShowOptions, NextNetActions.Continue);
    }
}