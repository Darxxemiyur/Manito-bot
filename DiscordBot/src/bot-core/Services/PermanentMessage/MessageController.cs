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
using Name.Bayfaderix.Darxxemiyur.Common;

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
            _postMessageUpdateQueue = new();
        }
        public async Task RunModule()
        {

            await Task.WhenAll(PostMessageUpdateLoop());
        }
        private async Task PostMessageUpdateLoop()
        {
            while (true)
            {
                var (id, tsk) = await _postMessageUpdateQueue.GetData();

                using var db = _service.Service.DbFactory.CreateMyDbContext();

                var translator = db.MessageWallTranslators
                    .Where(x => x.ID == id)
                    .Include(x => x.MessageWall)
                    .ThenInclude(x => x.Msgs)
                    .FirstOrDefault();


                if (translator != null)
                {
                    var updateResult = await translator.SubmitUpdate(_service.Client.Client);
                    tsk.SetResult(updateResult);
                    continue; // "yield" the loop to the next item.
                }

                tsk.SetException(new NullReferenceException(nameof(translator)));
            }
        }
        /// <summary>
        /// Posts an update request and return proxy for result.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Task<int?>> PostMessageUpdate(ulong id)
        {
            var callback = new TaskCompletionSource<int?>();

            await _postMessageUpdateQueue.Handle((id, callback));

            return callback.Task;
        }
        /// <summary>
        /// List of post update requests containing translator ID and a callback that resolves after update;
        /// </summary>
        private TaskEventProxy<(ulong, TaskCompletionSource<int?>)> _postMessageUpdateQueue;
        public Task StartSession(DiscordInteraction args)
        {
            return _service.StartSession(args, x => new MsgWallPanel(x));
        }
    }
}
