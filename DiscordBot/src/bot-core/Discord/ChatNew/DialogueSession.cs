using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Manito.Discord.Client;

using Name.Bayfaderix.Darxxemiyur.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Manito.Discord.ChatNew
{
	public struct SessionInnerMessage
	{
		public readonly object Generic;
		public readonly string Message;

		public SessionInnerMessage(object generic, string message) => (Generic, Message) = (generic, message);
	}

	public static class IDialogueExtender
	{
		public static Task<GeneralInteraction> GetInteraction(this IDialogueSession session, InteractionTypes types, CancellationToken token = default) => session.GetInteraction(types, token);
	}

	public interface IDialogueSession
	{
		/// <summary>
		/// My own discord client wrapper.
		/// </summary>
		MyDiscordClient Client {
			get;
		}

		/// <summary>
		/// Identifier of message to pull events for.
		/// </summary>
		IDialogueIdentifier Identifier {
			get;
		}

		/// <summary>
		/// Send/update session message.
		/// </summary>
		/// <param name="msg">The new message.</param>
		/// <returns></returns>
		Task SendMessage(UniversalMessageBuilder msg);

		/// <summary>
		/// Respond to an interaction to reply later.
		/// </summary>
		/// <returns></returns>
		Task DoLaterReply();

		/// <summary>
		/// Delete message.
		/// </summary>
		/// <returns></returns>
		Task RemoveMessage();

		/// <summary>
		/// Ends session.
		/// </summary>
		/// <returns></returns>
		Task EndSession();

		/// <summary>
		/// Gets message of the session.
		/// </summary>
		Task<DiscordMessage> SessionMessage {
			get;
		}

		/// <summary>
		/// Gets message of the session.
		/// </summary>
		Task<DiscordChannel> SessionChannel {
			get;
		}

		/// <summary>
		/// Gets message from user that is acceptable to SessionIdentifier.
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		Task<DiscordMessage> GetMessageInteraction(CancellationToken token = default);

		/// <summary>
		/// Gets reply message from user that is acceptable to SessionIdentifier.
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		Task<DiscordMessage> GetReplyInteraction(CancellationToken token = default);

		/// <summary>
		/// Get component interaction to a message that is acceptable to SessionIdentifier.
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		Task<InteractiveInteraction> GetComponentInteraction(CancellationToken token = default);

		/// <summary>
		/// Get interaction to a message that is acceptable to SessionIdentifier.
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task<GeneralInteraction> GetInteraction(InteractionTypes types, CancellationToken token = default)
		{
			CancellationTokenSource cancellation = new();
			token.Register(() => cancellation.Cancel());

			var tasks = new List<(InteractionTypes, Task)>();

			if (types.HasFlag(InteractionTypes.Component))
				tasks.Add((InteractionTypes.Component, GetComponentInteraction(cancellation.Token)));

			if (types.HasFlag(InteractionTypes.Message))
				tasks.Add((InteractionTypes.Message, GetMessageInteraction(cancellation.Token)));

			if (types.HasFlag(InteractionTypes.Reply))
				tasks.Add((InteractionTypes.Reply, GetReplyInteraction(cancellation.Token)));

			var first = await Task.WhenAny(tasks.Select(x => x.Item2));

			if (token.IsCancellationRequested)
				return new GeneralInteraction(InteractionTypes.Cancelled);

			if (cancellation.Token.CanBeCanceled)
				cancellation.Cancel();

			var couple = tasks.First(x => x.Item2 == first);

			return new GeneralInteraction(couple.Item1,
				couple.Item2 is Task<InteractiveInteraction> i ? await i : null,
				couple.Item2 is Task<DiscordMessage> m ? await m : null);
		}

		/// <summary>
		/// Used to inform subscribers about session status change.
		/// </summary>
		public event Func<IDialogueSession, SessionInnerMessage, Task> OnStatusChange;

		/// <summary>
		/// Used to inform subscribers about session end.
		/// </summary>
		public event Func<IDialogueSession, SessionInnerMessage, Task> OnSessionEnd;

		/// <summary>
		/// Used to inform subscribers about session removal.
		/// </summary>
		public event Func<IDialogueSession, Task<bool>> OnRemove;
	}

	public class UniversalSession : IDialogueSession
	{
		private IDialogueSession _innerSession;
		public MyDiscordClient Client => _innerSession.Client;
		public IDialogueIdentifier Identifier => _innerSession.Identifier;

		public UniversalSession(SessionFromMessage session)
		{
			_innerSession = session;
			((IDialogueSession)session).OnStatusChange += ConvertSession;
		}

		public UniversalSession(ComponentDialogueSession session) => _innerSession = session;

		private async Task ConvertSession(IDialogueSession session, SessionInnerMessage msg)
		{
			if (msg.Message != "ConvertMe")
				return;

			var msgp = ((DiscordInteraction, DialogueCompInterIdentifier, DiscordMessage))msg.Generic;
			_innerSession = new ComponentDialogueSession(Client, msgp.Item2, new InteractiveInteraction(msgp.Item1, msgp.Item3));

			session.OnStatusChange -= ConvertSession;
		}

		public event Func<IDialogueSession, SessionInnerMessage, Task> OnStatusChange;

		public event Func<IDialogueSession, SessionInnerMessage, Task> OnSessionEnd;

		public event Func<IDialogueSession, Task<bool>> OnRemove;

		public Task DoLaterReply() => _innerSession.DoLaterReply();

		public Task EndSession() => _innerSession.EndSession();

		public Task<InteractiveInteraction> GetComponentInteraction(CancellationToken token = default) => _innerSession.GetComponentInteraction(token);

		public Task<GeneralInteraction> GetInteraction(InteractionTypes types, CancellationToken token = default) => _innerSession.GetInteraction(types);

		public Task<DiscordMessage> GetMessageInteraction(CancellationToken token = default) => _innerSession.GetMessageInteraction(token);

		public Task<DiscordMessage> GetReplyInteraction(CancellationToken token = default) =>
			_innerSession.GetReplyInteraction(token);

		public Task RemoveMessage() => _innerSession.RemoveMessage();

		public Task SendMessage(UniversalMessageBuilder msg) => _innerSession.SendMessage(msg);

		public Task<DiscordMessage> SessionMessage => _innerSession.SessionMessage;

		public Task<DiscordChannel> SessionChannel => _innerSession.SessionChannel;
	}

	public class SessionFromMessage : IDialogueSession
	{
		private DiscordMessage _message;
		private DiscordChannel _channel;
		private ulong _userId;

		public SessionFromMessage(MyDiscordClient client, DiscordMessage message, ulong userId)
		{
			Client = client;
			Identifier = new DialogueMsgIdentifier(_message = message, _userId = userId);
			_channel = message.Channel;
		}

		public SessionFromMessage(MyDiscordClient client, DiscordChannel channel, ulong userId)
		{
			Client = client;
			_channel = channel;
			_userId = userId;
		}

		public MyDiscordClient Client {
			get;
		}

		public IDialogueIdentifier Identifier {
			get; private set;
		}

		public event Func<IDialogueSession, SessionInnerMessage, Task> OnStatusChange;

		public event Func<IDialogueSession, SessionInnerMessage, Task> OnSessionEnd;

		public event Func<IDialogueSession, Task<bool>> OnRemove;

		public async Task<DiscordMessage> GetMessageInteraction(CancellationToken token = default)
		{
			if (Identifier == null)
				throw new NotSupportedException();

			var msg = await Client.ActivityTools.WaitForMessage(x => Identifier.DoesBelongToUs(x.Message));

			return msg.Message;
		}

		public async Task<InteractiveInteraction> GetComponentInteraction(CancellationToken token = default)
		{
			if (Identifier == null)
				throw new NotSupportedException();
			InteractiveInteraction intr = await Client.ActivityTools.WaitForComponentInteraction(x => Identifier.DoesBelongToUs(x), token);

			await OnStatusChange(this, new SessionInnerMessage((intr.Interaction, new DialogueCompInterIdentifier(intr), _message), "ConvertMe"));

			return intr;
		}

		public Task<DiscordMessage> GetReplyInteraction(CancellationToken token = default)
		{
			throw new NotImplementedException();
		}

		public async Task SendMessage(UniversalMessageBuilder msg)
		{
			if (_message != null)
			{
				await _message.ModifyAsync(msg);
				return;
			}

			_message = await _channel.SendMessageAsync(msg);
			Identifier = new DialogueMsgIdentifier(_message, _userId);
		}

		public Task DoLaterReply() => Task.CompletedTask;

		public Task RemoveMessage() => _message.DeleteAsync();

		public async Task EndSession()
		{
			if (OnSessionEnd != null)
				await OnSessionEnd(this, new(null, "Ended"));
			if (OnRemove != null)
				await OnRemove(this);
		}

		public Task<DiscordMessage> SessionMessage => Task.FromResult(_message);

		public Task<DiscordChannel> SessionChannel => Task.FromResult(_channel);
	}

	public class ComponentDialogueSession : IDialogueSession
	{
		private InteractiveInteraction Interactive {
			get; set;
		}

		private UniversalMessageBuilder _innerMsgBuilder;
		private DiscordMessage _msg;

		public MyDiscordClient Client {
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

		public async Task RemoveMessage()
		{
			await using var _ = await _lock.BlockAsyncLock();
			try
			{
				await Interactive.Interaction.DeleteOriginalResponseAsync();
			}
			catch
			{
				try
				{
					var msg = await SessionMessage;
					await msg.DeleteAsync();
				}
				catch
				{
					await _msg.DeleteAsync();
				}
			}
		}

		public Task<DiscordMessage> GetReplyInteraction(CancellationToken token = default) => throw new NotImplementedException();

		public Task<DiscordMessage> SessionMessage => Interactive.Interaction.GetOriginalResponseAsync();

		public Task<DiscordChannel> SessionChannel => Task.Run(async () => await Client.Client.GetChannelAsync((await SessionMessage).ChannelId));

		public ComponentDialogueSession(MyDiscordClient client, DialogueCompInterIdentifier id, InteractiveInteraction interactive)
		{
			(Client, Interactive, Identifier) = (client, interactive, id);
			NextType = interactive.Message == null ? InteractionResponseType.ChannelMessageWithSource : InteractionResponseType.UpdateMessage;
		}

		public ComponentDialogueSession(MyDiscordClient client, DialogueCommandIdentifier id, InteractiveInteraction interactive)
		{
			(Client, Interactive, Identifier) = (client, interactive, id);
			NextType = interactive.Message == null ? InteractionResponseType.ChannelMessageWithSource : InteractionResponseType.UpdateMessage;
		}

		public ComponentDialogueSession(MyDiscordClient client, DiscordInteraction interaction)
		{
			Client = client;
			Identifier = new DialogueCommandIdentifier(Interactive = new(interaction));
			NextType = InteractionResponseType.ChannelMessageWithSource;
		}
	}
}