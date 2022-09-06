using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using DSharpPlus.SlashCommands.Attributes;

using Manito.Discord.Client;
using Name.Bayfaderix.Darxxemiyur.Common;

namespace Manito.Discord.Tickets
{
    public class TicketsFilter
    {
        public Task RunModule() => Task.WhenAll(HandleLoop(), RunHooked());
        private async Task HandleLoop()
        {
            while (true)
            {
                var data = (await _queue.GetData()).Item2;
                await HandleAsCommand(data.Item1, data.Item2);
            }
        }
        private async Task<ulong> CheckMessage(ulong chnlId, ulong msgId)
        {
            var clnt = _service.MyDiscordClient.Client;
            var chnl = await clnt.GetChannelAsync(chnlId);

            try
            {
                var msg = await chnl.GetMessageAsync(msgId);
            }
            catch (DSharpPlus.Exceptions.NotFoundException)
            {
                return (await chnl.SendMessageAsync(GetMsg())).Id;
            }

            return msgId;
        }
        private DiscordMessageBuilder GetMsg()
        {
            throw new NotImplementedException();
        }
        private async Task RunHooked()
        {
            ulong chnlId = 965561900517716008;
            ulong messageId = 966798532990345276;
            while (true)
            {
                messageId = await CheckMessage(chnlId, messageId);
                var inter = await _service.MyDiscordClient.ActivityTools
                    .WaitForComponentInteraction(x => x.Message.Id == messageId);

                var payload = (inter.Interaction, inter.Message.ChannelId);
                await _queue.Handle(_service.MyDiscordClient.Client, payload);
            }
        }
        private MyDomain _service;
        private List<DiscordApplicationCommand> _commandList;
        private DiscordEventProxy<(DiscordInteraction, ulong)> _queue;
        public TicketsFilter(MyDomain service, EventBuffer eventBuffer)
        {
            _service = service;
            _commandList = GetCommands().ToList();
            service.MyDiscordClient.AppCommands.Add("Shop", _commandList);
            _queue = new();
            eventBuffer.Interact.OnMessage += FilterMessage;
        }
        private IEnumerable<DiscordApplicationCommand> GetCommands()
        {
            yield return new DiscordApplicationCommand("shopping", "Начать шоппинг",
            defaultPermission: true);
        }

        private async Task FilterMessage(DiscordClient client, InteractionCreateEventArgs args)
        {
            if (_commandList.Any(x => args.Interaction.Data.Name.Contains(x.Name)))
            {
                await _queue.Handle(client, (args.Interaction, args.Interaction.ChannelId));
                args.Handled = true;
            }
        }
        private async Task HandleAsCommand(DiscordInteraction args, ulong channelId)
        {

        }
    }

}