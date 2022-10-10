using DisCatSharp.Entities;

using Manito.Discord.Client;

using Microsoft.EntityFrameworkCore;

using Name.Bayfaderix.Darxxemiyur.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Manito.Discord.Cleaning
{
	public class MessageRemover : IModule
	{
		private AsyncLocker _lock = new();
		private MyDomain _domain;
		private ICleaningDbFactory _dbFactory;

		public MessageRemover(MyDomain domain, ICleaningDbFactory dbFactory)
		{
			_domain = domain;
			_dbFactory = dbFactory;
		}

		public Task RemoveMessage(DiscordMessage message, DateTimeOffset time) => RemoveMessage(message.ChannelId, message.Id, time);

		public Task RemoveMessage(DiscordMessage message) => RemoveMessage(message.ChannelId, message.Id, DateTimeOffset.UtcNow);

		public async Task RemoveMessage(ulong channelId, ulong messageId, DateTimeOffset time)
		{
			await using var _ = await _lock.BlockAsyncLock();
			await using var db = await _dbFactory.CreateMyDbContextAsync();
			await db.MsgsToRemove.AddAsync(new MessageToRemove(messageId, channelId, time));
			await db.SaveChangesAsync();
		}

		public Task RemoveMessage(ulong channelId, ulong messageId) => RemoveMessage(channelId, messageId, DateTimeOffset.UtcNow);

		public Task RemoveMessage(IEnumerable<DiscordMessage> messages) => RemoveMessage(messages.Select(x => (x, DateTimeOffset.UtcNow)));

		public Task RemoveMessage(IEnumerable<(DiscordMessage, DateTimeOffset)> messages) => RemoveMessage(messages.ToDictionary(x => x.Item1.ChannelId, x => (x.Item1.Id, x.Item2)));

		public Task RemoveMessage(IDictionary<ulong, ulong> messages) => RemoveMessage(messages.ToDictionary(x => x.Key, x => (x.Value, DateTimeOffset.UtcNow)));

		public async Task RemoveMessage(IDictionary<ulong, (ulong, DateTimeOffset)> messages)
		{
			await using var _ = await _lock.BlockAsyncLock();
			await using var db = await _dbFactory.CreateMyDbContextAsync();
			await db.MsgsToRemove.AddRangeAsync(messages.Select(x => new MessageToRemove(x.Value.Item1, x.Key, x.Value.Item2)));
			await db.SaveChangesAsync();
		}

		public async Task RunModule()
		{
			while (true)
			{
				var delayStart = DateTimeOffset.UtcNow;
				var span = TimeSpan.FromMilliseconds(1250);
				try
				{
					await using var _ = await _lock.BlockAsyncLock();
					await using var db = await _dbFactory.CreateMyDbContextAsync();

					var msgs = await db.MsgsToRemove.Where(x => x.Expiration <= DateTimeOffset.UtcNow).ToListAsync();
					var attemptAgain = new List<MessageToRemove>(msgs.Count);
					var toClear = new List<MessageToRemove>(msgs.Count);

					foreach (var msg in msgs)
					{
						if (await AttemptToRemove(msg))
						{
							toClear.Add(msg);
						}
						else
						{
							msg.Expiration = delayStart + TimeSpan.FromMinutes(2);
							msg.TimesFailed += 1;
							attemptAgain.Add(msg);
						}
					}

					db.MsgsToRemove.RemoveRange(toClear);
					db.MsgsToRemove.UpdateRange(attemptAgain);

					await db.SaveChangesAsync();
				}
				catch { }
				await Task.Delay(TimeSpan.FromMilliseconds(Math.Max(1, (delayStart + span - DateTimeOffset.UtcNow).TotalMilliseconds)));
			}
		}

		private async Task<bool> AttemptToRemove(MessageToRemove msgt)
		{
			try
			{
				var client = _domain.MyDiscordClient.Client;

				var channel = await client.GetChannelAsync(msgt.ChannelID);

				var message = await channel.GetMessageAsync(msgt.MessageID, true);

				await message.DeleteAsync();
				return true;
			}
			catch { }
			return msgt.TimesFailed > 20;
		}
	}
}