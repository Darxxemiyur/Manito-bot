using System;
using Microsoft.EntityFrameworkCore;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using Manito.Discord.Database;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Manito.Discord.Client;

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
        public async Task SubmitUpdate(DiscordChannel channel)
        {
            var oldDict = Translation;
            var mwDict = MessageWall.Msgs;
            Translation = new();

            var length = Math.Max(oldDict.Count, mwDict.Count);

            for (var i = 0; i < length; i++)
            {
                var tgt = mwDict.ElementAtOrDefault(i).WallLine;
                var slv = oldDict.ElementAtOrDefault(i);

                if (tgt == slv.Value)
                {
                    Translation.Add(slv.Key, slv.Value);
                    continue;
                }

                if (slv.Value == null)
                {
                    var replyK = Translation.ElementAtOrDefault(0);
                    var reply = replyK.Key == default ? null : (ulong?)replyK.Key;
                    var myMsg = await channel.SendMessageAsync(
                        new DiscordMessageBuilder().WithReply(reply)
                        .WithContent(reply != null ? null : MessageWall.WallName)
                        .WithEmbed(new DiscordEmbedBuilder().WithDescription(tgt))
                    );
                    Translation.Add(myMsg.Id, tgt);
                    continue;
                }

                var msg = await channel.GetMessageAsync(slv.Key);

                if (tgt == null)
                {
                    await channel.DeleteMessageAsync(msg);
                    continue;
                }
                if (tgt != slv.Value)
                {
                    Translation.Add(msg.Id, tgt);
                    await msg.ModifyAsync(x => (x.Content, x.Embed) =
                     (msg.Content, new DiscordEmbedBuilder(msg.Embeds[0]).WithDescription(tgt)));
                }
            }
        }
    }
}
