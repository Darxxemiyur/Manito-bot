using DisCatSharp.Entities;
using DisCatSharp.Enums;

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
		private DiscordChannel _chnl;

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

		private bool _isHazard;

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
			try
			{
				await SendMessageLocal(message);
			}
			catch (Exception e)
			{
				await Client.Domain.Logging.WriteErrorClassedLog(GetType().Name, e, true);
				await FallBackToMessageSession();
				throw;
			}
		}

		private async Task SendMessageLocal(UniversalMessageBuilder message)
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

		private async Task MarkTheMessage(DiscordMessage msg = default)
		{
			_msg = msg ?? await SessionMessage;
			_chnl = _msg.Channel;
			Identifier = new DialogueCompInterIdentifier(new(Interactive.Interaction, _msg));
		}

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
			switch (NextType)
			{
				case InteractionResponseType.ChannelMessageWithSource:
					await Interactive.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
					NextType = InteractionResponseType.Pong;
					await MarkTheMessage();
					break;

				case InteractionResponseType.UpdateMessage:
					await Interactive.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
					NextType = InteractionResponseType.Pong;
					break;
			}
		}

		private async Task FallBackToMessageSession()
		{
			var chnl = await Client.Client.GetChannelAsync(Identifier.ChannelId);
			try
			{
				var msg = await chnl.GetMessageAsync(Identifier.MessageId, true);
				await OnStatusChange(this, new SessionInnerMessage((msg, Identifier.UserId), "ConvertMeToMsg1"));
			}
			catch (Exception ev)
			{
				await Client.Domain.Logging.WriteErrorClassedLog(GetType().Name, ev, true);
				await OnStatusChange(this, new SessionInnerMessage((chnl, Identifier.UserId), "ConvertMeToMsg2"));
			}
		}

		public async Task DoLaterReply()
		{
			await using var _ = await _lock.BlockAsyncLock();
			try
			{
				if (Interactive?.Components?.Any() == true)
					await CancelClickability();
				else
					await RespondToAnInteraction();
			}
			catch (Exception e)
			{
				await Client.Domain.Logging.WriteErrorClassedLog(GetType().Name, e, true);
				await FallBackToMessageSession();
				throw;
			}
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
					await FallBackToMessageSession();
					throw;
				}
			}
		}

		public async Task RemoveMessage()
		{
			await using var _ = await _lock.BlockAsyncLock();
			await RemoveMessageLocal();
		}

		public Task<DiscordMessage> GetReplyInteraction(CancellationToken token = default) => throw new NotImplementedException();

		public UniversalSession ToUniversal() => (UniversalSession)this;

		public Task<DiscordMessage> SessionMessage => Interactive.Interaction.GetOriginalResponseAsync();

		public Task<DiscordChannel> SessionChannel => Task.Run(async () => await Client.Client.GetChannelAsync((await SessionMessage).ChannelId));

		public ComponentDialogueSession(MyClientBundle client, DialogueCompInterIdentifier id, InteractiveInteraction interactive)
		{
			(Client, Interactive, Identifier) = (client, interactive, id);
			NextType = id.MessageId == 0 ? InteractionResponseType.ChannelMessageWithSource : InteractionResponseType.UpdateMessage;
		}

		public ComponentDialogueSession(MyClientBundle client, DialogueCommandIdentifier id, InteractiveInteraction interactive)
		{
			(Client, Interactive, Identifier) = (client, interactive, id);
			NextType = id.MessageId == 0 ? InteractionResponseType.ChannelMessageWithSource : InteractionResponseType.UpdateMessage;
		}

		public ComponentDialogueSession(MyClientBundle client, DiscordInteraction interaction)
		{
			Client = client;
			Identifier = new DialogueCommandIdentifier(Interactive = new(interaction));
			NextType = InteractionResponseType.ChannelMessageWithSource;
		}

		public static implicit operator UniversalSession(ComponentDialogueSession msg) => new(msg);
	}
}