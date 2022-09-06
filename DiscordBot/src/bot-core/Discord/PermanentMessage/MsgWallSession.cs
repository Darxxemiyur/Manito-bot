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
    public class MessageWallSession : DialogueNetSession
    {
        public readonly IPermMessageDbFactory DBFactory;
        public MessageWallSession(InteractiveInteraction iargs,
         MyDiscordClient client, DiscordUser user, IPermMessageDbFactory factory)
         : base(iargs, client, user, false)
        {
            DBFactory = factory;
        }
        
    }
    public class MessageWallSessionController : DialogueNetSessionControls<MessageWallSession>
    {
        public MessageWallSessionController(MyDomain service) : base(service)
        {

        }
        public Task<MessageWallSession> StartSession(DiscordInteraction args,
         Func<MessageWallSession, IDialogueNet> getNet) => StartSession(() =>
         new MessageWallSession(new(args), Service.MyDiscordClient, args.User, Service.DbFactory), getNet);
    }
}