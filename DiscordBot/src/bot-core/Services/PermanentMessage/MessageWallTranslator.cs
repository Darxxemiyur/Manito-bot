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
using Cyriller;
using System.Collections.Immutable;

namespace Manito.Discord.PermanentMessage
{

	public class MessageWallTranslator
	{
		public ulong ID {
			get; set;
		}
		public MessageWall MessageWall {
			get; set;
		}
		public ulong ChannelId {
			get; set;
		}
		private static string Rp => ":YYvYYgYYvYY:";
		public string CTranslation {
			get => string.Join(Rp, Translation ?? new List<ulong>());
			set => Translation = value.Split(Rp).Where(y => ulong.TryParse(y, out var v))
				.Select(y => ulong.Parse(y)).ToList();
		}

		/// <summary>
		/// List of message id to content pairs.
		/// </summary>
		/// <value></value>
		public List<ulong> Translation {
			get; set;
		}
		public MessageWallTranslator()
		{
		}
		public MessageWallTranslator(MessageWall messageWall, ulong channelId)
		{
			Translation = new();
			ChannelId = channelId;
			MessageWall = messageWall;
		}
		public async Task<int?> SubmitUpdate(DiscordClient client)
		{
			var oldDict = Translation;
			oldDict.Sort();
			var mwDict = MessageWall.Msgs.OrderBy(x => x.ID).ToList();

			DiscordChannel channel = null;

			try
			{
				channel = await client.GetChannelAsync(ChannelId);
			}
			catch (NotFoundException) { return null; }

			Translation = new();

			var rerun = false;

			var length = Math.Max(oldDict.Count, mwDict.Count);
			var changed = length;

			for (var i = 0; i < length; i++)
			{
				var slv = oldDict.ElementAtOrDefault(i);
				var tgt = mwDict.ElementAtOrDefault(i).WallLine;
				tgt = tgt[..Math.Min(2000, tgt.Length)];

				DiscordMessage msg = null;
				string content = null;
				try
				{
					msg = await channel.GetMessageAsync(slv);
					content = msg?.Embeds[0]?.Description;
				}
				catch
				{
					msg = await CreateMessage(channel, tgt);
					Translation.Add(msg.Id);
					rerun = true;
					continue;
				}
				if (tgt.IsNullOrEmpty())
				{
					await channel.DeleteMessageAsync(msg);
					continue;
				}
				if (tgt == content)
				{
					Translation.Add(msg.Id);
					changed--;
					continue;
				}
				if (tgt != content)
				{
					Translation.Add(msg.Id);
					await msg.ModifyAsync(x => (x.Content, x.Embed) =
						("", new DiscordEmbedBuilder(msg.Embeds[0]).WithDescription(tgt)));
				}
			}

			return rerun ? Math.Max(await SubmitUpdate(client) ?? 0, changed) : changed;
		}

		private async Task<DiscordMessage> CreateMessage(DiscordChannel channel, string tgt)
		{
			var myMsg = await channel.SendMessageAsync(
				new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder().WithDescription(tgt))
			);

			return myMsg;
		}
	}
}
