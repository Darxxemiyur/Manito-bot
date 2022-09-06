using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Emzi0767.Utilities;
using DSharpPlus.Interactivity.EventHandling;
using System.Threading;

namespace Manito.Discord.Client
{
	public class ActivitiesTools
	{
		private EventInline _evInline;
		public ActivitiesTools(EventInline evInline) {
			_evInline = evInline;
		}

		public Task<MessageReactionAddEventArgs> WaitForReaction(DiscordMessage message)
			=> WaitForReaction((MessageReactionAddEventArgs x) => x.Message.Id == message.Id);
		public async Task<MessageCreateEventArgs> WaitForMessage(
			Func<MessageCreateEventArgs, bool> pred) {
			var catcher = new SingleEventCatcher<MessageCreateEventArgs>(pred);
			await _evInline.MessageBuffer.Add(catcher);
			var evnv = await catcher.GetEvent();

			return evnv.Item2;
		}
		public Task<ComponentInteractionCreateEventArgs> WaitForComponentInteraction(
			DiscordMessage message) => WaitForComponentInteraction((x) => x.Message.Id == message.Id);
		public async Task<ComponentInteractionCreateEventArgs> WaitForComponentInteraction(
			Func<ComponentInteractionCreateEventArgs, bool> checker) {
			var catcher = new SingleEventCatcher<ComponentInteractionCreateEventArgs>(checker);
			await _evInline.CompInteractBuffer.Add(catcher);
			var evnv = await catcher.GetEvent();

			return evnv.Item2;
		}
		public async Task<ComponentInteractionCreateEventArgs> WaitForComponentInteraction(
			Func<ComponentInteractionCreateEventArgs, bool> checker, TimeSpan timeout, CancellationToken token) {
			var catcher = new SingleEventCatcher<ComponentInteractionCreateEventArgs>(checker);
			await _evInline.CompInteractBuffer.Add(catcher);
			var evnv = await catcher.GetEvent(timeout, token);

			return evnv.Item2;
		}
		public async Task<ComponentInteractionCreateEventArgs> WaitForComponentInteraction(
			Func<ComponentInteractionCreateEventArgs, bool> checker, TimeSpan timeout) {
			var catcher = new SingleEventCatcher<ComponentInteractionCreateEventArgs>(checker);
			await _evInline.CompInteractBuffer.Add(catcher);
			var evnv = await catcher.GetEvent(timeout);

			return evnv.Item2;
		}
		public async Task<ComponentInteractionCreateEventArgs> WaitForComponentInteraction(
			Func<ComponentInteractionCreateEventArgs, bool> checker, CancellationToken token) {
			var catcher = new SingleEventCatcher<ComponentInteractionCreateEventArgs>(checker);
			await _evInline.CompInteractBuffer.Add(catcher);
			var evnv = await catcher.GetEvent(token);

			return evnv.Item2;
		}
		public async Task<MessageReactionAddEventArgs> WaitForReaction(
			Func<MessageReactionAddEventArgs, bool> pred) {
			var catcher = new SingleEventCatcher<MessageReactionAddEventArgs>(pred);
			await _evInline.ReactAddBuffer.Add(catcher);
			var evnv = await catcher.GetEvent();

			return evnv.Item2;
		}
		public async Task<MessageReactionAddEventArgs> WaitForReaction(
			Func<MessageReactionAddEventArgs, bool> pred, TimeSpan timeout) {
			var catcher = new SingleEventCatcher<MessageReactionAddEventArgs>(pred);
			await _evInline.ReactAddBuffer.Add(catcher);
			var evnv = await catcher.GetEvent(timeout);

			return evnv.Item2;
		}
		public async Task<MessageCreateEventArgs> WaitForMessage(
			Func<MessageCreateEventArgs, bool> pred, TimeSpan timeout) {
			var catcher = new SingleEventCatcher<MessageCreateEventArgs>(pred);
			await _evInline.MessageBuffer.Add(catcher);
			var evnv = await catcher.GetEvent(timeout);

			return evnv.Item2;
		}

	}
}