using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.EventArgs;
using Manito.Discord.Chat.DialogueNet;
using Manito.Discord.Client;
using Name.Bayfaderix.Darxxemiyur.Common;


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