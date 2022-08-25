using System;
using Microsoft.EntityFrameworkCore;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using Manito.Discord.Database;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Manito.Discord.Client;
using DSharpPlus;
using DSharpPlus.Exceptions;

namespace Manito.Discord.PermanentMessage
{

    public class MessageWallTranslator
    {
        public ulong ID { get; set; }
        public MessageWall MessageWall { get; set; }
        public ulong ChannelId { get; set; }
        public string CTranslation
        {
            get => string.Join(";", Translation.Select(y => $"{y.Key}:{y.Value}"));
            set => Translation = value.Split(";").Select(y => y.Split(":"))
                    .ToDictionary(y => ulong.Parse(y[0]), y => y[1]);
        }

        /// <summary>
        /// List of message id to content pairs.
        /// </summary>
        /// <value></value>
        public Dictionary<ulong, string> Translation { get; set; }
        public MessageWallTranslator() { }
        public MessageWallTranslator(MessageWall messageWall, ulong channelId)
        {
            Translation = new();
            ChannelId = channelId;
            MessageWall = messageWall;
        }
        public async Task<int?> SubmitUpdate(DiscordClient client)
        {
            var oldDict = Translation;
            var mwDict = MessageWall.Msgs;

            DiscordChannel channel = null;

            try { channel = await client.GetChannelAsync(ChannelId); }
            catch (NotFoundException) { return null; }

            Translation = new();

            var length = Math.Max(oldDict.Count, mwDict.Count);
            var changed = length;

            for (var i = 0; i < length; i++)
            {
                var tgt = mwDict.ElementAtOrDefault(i).WallLine;
                var slv = oldDict.ElementAtOrDefault(i);

                if (tgt == slv.Value)
                {
                    Translation.Add(slv.Key, slv.Value);
                    changed--;
                    continue;
                }

                if (slv.Value == null)
                {
                    DiscordMessage myMsg = await CreateMessage(channel, tgt);
                    Translation.Add(myMsg.Id, tgt);
                    continue;
                }

                if (tgt == null)
                {
                    try
                    {
                        var msg = await channel.GetMessageAsync(slv.Key);
                        await channel.DeleteMessageAsync(msg);
                    }
                    catch (NotFoundException) { }
                    continue;
                }
                if (tgt != slv.Value)
                {
                    DiscordMessage msg = null;
                    try
                    {
                        msg = await channel.GetMessageAsync(slv.Key);
                        await msg.ModifyAsync(x => (x.Content, x.Embed) =
                            (msg.Content, new DiscordEmbedBuilder(msg.Embeds[0]).WithDescription(tgt)));
                    }
                    catch (NotFoundException) { }

                    msg ??= await CreateMessage(channel, tgt);

                    Translation.Add(msg.Id, tgt);
                }
            }

            return changed;
        }

        private async Task<DiscordMessage> CreateMessage(DiscordChannel channel, string tgt)
        {
            var replyK = Translation.ElementAtOrDefault(0);
            var reply = replyK.Key == default ? null : (ulong?)replyK.Key;
            var myMsg = await channel.SendMessageAsync(
                new DiscordMessageBuilder()
                .WithContent(reply != null ? null : MessageWall.WallName)
                .WithEmbed(new DiscordEmbedBuilder().WithDescription(tgt))
            );

            return myMsg;
        }
    }
}
