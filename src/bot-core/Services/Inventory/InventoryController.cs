using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using Manito.Discord.Chat.DialogueNet;
using Manito.Discord.Client;


namespace Manito.Discord.Inventory
{
    public class InventoryController : DialogueNetSessionControls<InventorySession>
    {
        public InventoryController(MyDomain service) : base(service)
        {

        }
        public Task<InventorySession> StartSession(DiscordInteraction args,
         Func<InventorySession, IDialogueNet> getNet) => StartSession(() =>
         new InventorySession(new(args), Service.MyDiscordClient, Service.Inventory, args.User, this), getNet);
    }

}