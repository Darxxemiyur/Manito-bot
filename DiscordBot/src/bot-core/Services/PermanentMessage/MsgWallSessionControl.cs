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
using Name.Bayfaderix.Darxxemiyur.Common;


namespace Manito.Discord.PermanentMessage
{
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