using System;
using Microsoft.EntityFrameworkCore;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using Manito.Discord.Database;
using System.Threading.Tasks;
using Manito.Discord.Client;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus;
using Manito.Discord.Chat.DialogueNet;
using Name.Bayfaderix.Darxxemiyur.Node.Network;
using DSharpPlus.EventArgs;

namespace Manito.Discord.PermanentMessage
{

    public class MessageController : IModule
    {
        private IPermMessageDbFactory _dbFactory;
        private MyDomain _domain;
        private MessageWallSessionController _service;
        public MessageController(MyDomain domain)
        {
            _service = new(domain);
            _domain = domain;
            _dbFactory = domain.DbFactory;
        }
        public async Task RunModule()
        {
            
        }
        public async Task StartSession(DiscordInteraction args)
        {
            await _service.StartSession(args, x => new MsgWallPanel(x));
        }
    }
}
