using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Exceptions;

using Manito.Discord.Client;

using Name.Bayfaderix.Darxxemiyur.Common;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Manito.Discord.ChatNew
{
	public class ComponentDialogueSession : IDialogueSession
	{
		private InteractiveInteraction Interactive {
			get; set;
		}

		private UniversalMessageBuilder _innerMsgBuilder;
		private DiscordMessage _msg;

		public MyClientBundle Client {
			get;
		}

		public IDialogueIdentifier Identifier {
			get; private set;
		}

		public InteractionResponseType NextType {
			get; private set;
		}

		private readonly AsyncLocker _lock = new();

		/// <summary>
		/// Used to inform subscribers about session status change.
		/// </summary>
		public event Func<IDialogueSession, SessionInnerMessage, Task> OnStatusChange;

		public event Func<IDialogueSession, SessionInnerMessage, Task> OnSessionEnd;

		public event Func<IDialogueSession, Task<bool>> OnRemove;

		public async Task EndSession()
		{
			if (OnSessionEnd != null)
				await OnSessionEnd(this, new(null, "Ended"));
			if (OnRemove != null)
				await OnRemove(this);
		}

		public async Task SendMessage(UniversalMessageBuilder message)
		{
			await using var _ = await _lock.BlockAsyncLock();
			await SendMessageLocal(message);
		}

		private async Task SendMessageLocal(UniversalMessageBuilder message)
		{
			for (var times = 0; ; times++)
			{
				try
				{
					switch (NextType)
					{
						case InteractionResponseType.ChannelMessageWithSource:
							await Interactive.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, message);
							NextType = InteractionResponseType.Pong;
							await MarkTheMessage();
							break;

						case InteractionResponseType.UpdateMessage:
							await Interactive.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, message);
							NextType = InteractionResponseType.Pong;
							break;

						case InteractionResponseType.Pong:
							await MarkTheMessage(await Interactive.Interaction.EditOriginalResponseAsync(message));
							break;
					}
					_innerMsgBuilder = message;
				}
				catch (Exception e)
				{
					if (ErrorMaxRepeatTimes < times)
						throw;
					await Client.Domain.Logging.WriteErrorClassedLog(GetType().Name, e, true);
					await Task.Delay(ErrorDelayTime);
					continue;
				}
				break;
			}
		}

		private async Task MarkTheMessage(DiscordMessage msg = default) => Identifier = new DialogueCompInterIdentifier(new(Interactive.Interaction, _msg = msg ?? await SessionMessage));

		private async Task CancelClickability()
		{
			var msg = _innerMsgBuilder;
			var builder = new UniversalMessageBuilder(msg);

			var components = builder.Components.Select(y => y.Select(x => {
				if (x is DiscordButtonComponent f)
					return new DiscordButtonComponent(f).Disable();
				if (x is DiscordSelectComponent g)
					return new DiscordSelectComponent(g.Placeholder, g.Options, g.CustomId, (int)g.MinimumSelectedValues, (int)g.MaximumSelectedValues, true);
				return x;
			}).ToArray()).ToArray();

			await SendMessageLocal(builder.SetComponents(components));
			_innerMsgBuilder = msg;
			NextType = InteractionResponseType.Pong;
		}

		private async Task RespondToAnInteraction()
		{
			for (var times = 0; ; times++)
			{
				try
				{
					switch (NextType)
					{
						case InteractionResponseType.ChannelMessageWithSource:
							await Interactive.Interaction
								.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
							NextType = InteractionResponseType.Pong;
							await MarkTheMessage();
							break;

						case InteractionResponseType.UpdateMessage:
							await Interactive.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
							NextType = InteractionResponseType.Pong;
							break;
					}
				}
				catch (Exception e)
				{
					if (ErrorMaxRepeatTimes < times)
						throw;
					await Client.Domain.Logging.WriteErrorClassedLog(GetType().Name, e, true);
					await Task.Delay(ErrorDelayTime);
					continue;
				}
				break;
			}
		}

		public async Task DoLaterReply()
		{
			await using var _ = await _lock.BlockAsyncLock();
			if (Interactive?.Components?.Any() == true)
				await CancelClickability();
			else
				await RespondToAnInteraction();
		}

		public async Task<DiscordMessage> GetMessageInteraction(CancellationToken token = default)
		{
			var msg = await Client.ActivityTools.WaitForMessage(x => Identifier.DoesBelongToUs(x.Message), token);

			return msg.Message;
		}

		public async Task<InteractiveInteraction> GetComponentInteraction(CancellationToken token = default)
		{
			InteractiveInteraction intr = await Client.ActivityTools.WaitForComponentInteraction(x => Identifier.DoesBelongToUs(x), token);

			await using var _ = await _lock.BlockAsyncLock();
			Identifier = new DialogueCompInterIdentifier(Interactive = intr);
			NextType = InteractionResponseType.UpdateMessage;

			return intr;
		}

		private async Task RemoveMessageLocal()
		{
			for (var times = 0; ; times++)
			{
				try
				{
					await Interactive.Interaction.DeleteOriginalResponseAsync();
				}
				catch (Exception e1)
				{
					await Client.Domain.Logging.WriteErrorClassedLog(GetType().Name, e1, true);
					try
					{
						var msg = await SessionMessage;
						await msg.DeleteAsync();
					}
					catch (Exception e2)
					{
						await Client.Domain.Logging.WriteErrorClassedLog(GetType().Name, e2, true);
						try
						{
							await _msg.DeleteAsync();
						}
						catch (Exception e3)
						{
							if (ErrorMaxRepeatTimes < times)
								throw new AggregateException(e1, e2, e3);
							await Client.Domain.Logging.WriteErrorClassedLog(GetType().Name, e3, true);
							await Task.Delay(ErrorDelayTime);
							continue;
						}
					}
				}
				break;
			}
		}

		private static TimeSpan ErrorDelayTime = TimeSpan.FromSeconds(10);
		private static int ErrorMaxRepeatTimes = 360;

		public async Task RemoveMessage()
		{
			await using var _ = await _lock.BlockAsyncLock();
			await RemoveMessageLocal();
		}

		public Task<DiscordMessage> GetReplyInteraction(CancellationToken token = default) => throw new NotImplementedException();

		public Task<DiscordMessage> SessionMessage => Interactive.Interaction.GetOriginalResponseAsync();

		public Task<DiscordChannel> SessionChannel => Task.Run(async () => await Client.Client.GetChannelAsync((await SessionMessage).ChannelId));

		public ComponentDialogueSession(MyClientBundle client, DialogueCompInterIdentifier id, InteractiveInteraction interactive)
		{
			(Client, Interactive, Identifier) = (client, interactive, id);
			NextType = interactive.Message == null ? InteractionResponseType.ChannelMessageWithSource : InteractionResponseType.UpdateMessage;
		}

		public ComponentDialogueSession(MyClientBundle client, DialogueCommandIdentifier id, InteractiveInteraction interactive)
		{
			(Client, Interactive, Identifier) = (client, interactive, id);
			NextType = interactive.Message == null ? InteractionResponseType.ChannelMessageWithSource : InteractionResponseType.UpdateMessage;
		}

		public ComponentDialogueSession(MyClientBundle client, DiscordInteraction interaction)
		{
			Client = client;
			Identifier = new DialogueCommandIdentifier(Interactive = new(interaction));
			NextType = InteractionResponseType.ChannelMessageWithSource;
		}
	}
}