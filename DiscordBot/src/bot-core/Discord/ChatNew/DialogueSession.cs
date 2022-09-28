using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Manito.Discord.Client;

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.VisualBasic;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		Task<GeneralInteraction> GetInteraction(InteractionTypes types);
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
	public class SessionFromMessage : IDialogueSession
	{
		private DiscordMessage _message;
		public SessionFromMessage(MyDiscordClient client, DiscordMessage message, ulong userId)
		{
			Client = client;
			Identifier = new DialogueMsgIdentifier(_message = message, userId);
		}
		public MyDiscordClient Client {
			get;
		}
		public IDialogueIdentifier Identifier {
			get;
		}
		public event Func<IDialogueSession, SessionInnerMessage, Task> OnStatusChange;
		public event Func<IDialogueSession, SessionInnerMessage, Task> OnSessionEnd;
		public event Func<IDialogueSession, Task<bool>> OnRemove;

		public async Task<DiscordMessage> GetMessageInteraction(CancellationToken token = default)
		{
			var msg = await Client.ActivityTools
				.WaitForMessage(x => Identifier.DoesBelongToUs(x.Message));

			return msg.Message;
		}
		public async Task<InteractiveInteraction> GetComponentInteraction(CancellationToken token = default)
		{
			InteractiveInteraction intr = await Client.ActivityTools
				.WaitForComponentInteraction(x => Identifier.DoesBelongToUs(x), token);

			await OnStatusChange(this, new SessionInnerMessage((intr.Interaction, _message), "ConvertMe"));

			return intr;
		}
		/// <summary>
		/// Get any interaction, andd remove hooks if one arrived faster than the rest.
		/// </summary>
		/// <param name="types"></param>
		/// <returns></returns>
		public async Task<GeneralInteraction> GetInteraction(InteractionTypes types)
		{
			CancellationTokenSource cancellation = new();

			var tasks = new List<(InteractionTypes, Task)>();
			if (types.HasFlag(InteractionTypes.Component))
			{
				tasks.Add((InteractionTypes.Component, GetComponentInteraction(cancellation.Token)));
			}
			if (types.HasFlag(InteractionTypes.Message))
			{
				tasks.Add((InteractionTypes.Message, GetMessageInteraction(cancellation.Token)));
			}

			var first = await Task.WhenAny(tasks.Select(x => x.Item2));

			cancellation.Cancel();

			var couple = tasks.First(x => x.Item2 == first);


			return new GeneralInteraction(couple.Item1,
				couple.Item2 is Task<InteractiveInteraction> i ? await i : null,
				couple.Item2 is Task<DiscordMessage> m ? await m : null);
		}
		public Task<DiscordMessage> GetReplyInteraction(CancellationToken token = default) => throw new NotImplementedException();
		public Task SendMessage(UniversalMessageBuilder msg) => _message.ModifyAsync(msg);
		public Task DoLaterReply() => Task.CompletedTask;
		public Task RemoveMessage() => _message.DeleteAsync();
		public Task EndSession() => throw new NotImplementedException();
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
		public UniversalSession(ComponentDialogueSession session)
		{
			_innerSession = session;
		}
		private async Task ConvertSession(IDialogueSession session, SessionInnerMessage msg)
		{
			if (msg.Message != "ConvertMe")
				return;

			var msgp = ((DiscordInteraction, DiscordMessage))msg.Generic;
			_innerSession = new ComponentDialogueSession(Client, msgp.Item1, msgp.Item2);

			session.OnStatusChange -= ConvertSession;
		}

		public event Func<IDialogueSession, SessionInnerMessage, Task> OnStatusChange;
		public event Func<IDialogueSession, SessionInnerMessage, Task> OnSessionEnd;
		public event Func<IDialogueSession, Task<bool>> OnRemove;

		public Task DoLaterReply() => _innerSession.DoLaterReply();
		public Task EndSession() => _innerSession.EndSession();
		public Task<InteractiveInteraction> GetComponentInteraction(CancellationToken token = default) => _innerSession.GetComponentInteraction(token);
		public Task<GeneralInteraction> GetInteraction(InteractionTypes types) => _innerSession.GetInteraction(types);
		public Task<DiscordMessage> GetMessageInteraction(CancellationToken token = default) => _innerSession.GetMessageInteraction(token);
		public Task<DiscordMessage> GetReplyInteraction(CancellationToken token = default) =>
			_innerSession.GetReplyInteraction(token);
		public Task RemoveMessage() => _innerSession.RemoveMessage();
		public Task SendMessage(UniversalMessageBuilder msg) => _innerSession.SendMessage(msg);
	}
	public class ComponentDialogueSession : IDialogueSession
	{
		private InteractiveInteraction Interactive {
			get; set;
		}
		public MyDiscordClient Client {
			get;
		}
		public IDialogueIdentifier Identifier {
			get; private set;
		}
		public InteractionResponseType NextType {
			get; private set;
		}
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
			switch (NextType)
			{
				case InteractionResponseType.ChannelMessageWithSource:
					await Interactive.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, message);
					NextType = InteractionResponseType.Pong;
					break;
				case InteractionResponseType.UpdateMessage:
					await Interactive.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, message);
					NextType = InteractionResponseType.Pong;
					break;
				case InteractionResponseType.Pong:
					await Interactive.Interaction.EditOriginalResponseAsync(message);
					break;
			}
			await UpdateIdentifier();
		}
		private async Task UpdateIdentifier()
		{
			var msg = await Interactive.Interaction.GetOriginalResponseAsync();
			Identifier = new DialogueMessageIdentifier(new(Interactive.Interaction, msg));
		}
		private async Task CancelClickability()
		{
			var builder = new UniversalMessageBuilder(Interactive.Message);

			var components = builder.Components.Select(x => x.Where(x => x is DiscordButtonComponent)
			.Select(x => new DiscordButtonComponent((DiscordButtonComponent)x).Disable()).ToArray()).ToArray();

			builder.SetComponents(components);

			await SendMessage(builder);
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
					break;
				case InteractionResponseType.UpdateMessage:
					await Interactive.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
					NextType = InteractionResponseType.Pong;
					break;
			}
			await UpdateIdentifier();
		}
		public async Task DoLaterReply()
		{
			if (Interactive?.Components?.Any() == true)
				await CancelClickability();
			else
				await RespondToAnInteraction();
		}

		public async Task<DiscordMessage> GetMessageInteraction(CancellationToken token = default)
		{
			var msg = await Client.ActivityTools
				.WaitForMessage(x => Identifier.DoesBelongToUs(x.Message));

			return msg.Message;
		}
		public async Task<InteractiveInteraction> GetComponentInteraction(CancellationToken token = default)
		{
			InteractiveInteraction intr = await Client.ActivityTools
				.WaitForComponentInteraction(x => Identifier.DoesBelongToUs(x), token);

			Interactive = intr;
			NextType = InteractionResponseType.UpdateMessage;

			return intr;
		}
		/// <summary>
		/// Get any interaction, andd remove hooks if one arrived faster than the rest.
		/// </summary>
		/// <param name="types"></param>
		/// <returns></returns>
		public async Task<GeneralInteraction> GetInteraction(InteractionTypes types)
		{
			CancellationTokenSource cancellation = new();

			var tasks = new List<(InteractionTypes, Task)>();
			if (types.HasFlag(InteractionTypes.Component))
			{
				tasks.Add((InteractionTypes.Component, GetComponentInteraction(cancellation.Token)));
			}
			if (types.HasFlag(InteractionTypes.Message))
			{
				tasks.Add((InteractionTypes.Message, GetMessageInteraction(cancellation.Token)));
			}

			var first = await Task.WhenAny(tasks.Select(x => x.Item2));

			cancellation.Cancel();

			var couple = tasks.First(x => x.Item2 == first);


			return new GeneralInteraction(couple.Item1,
				couple.Item2 is Task<InteractiveInteraction> i ? await i : null,
				couple.Item2 is Task<DiscordMessage> m ? await m : null);
		}

		public Task RemoveMessage() => Interactive.Interaction.DeleteOriginalResponseAsync();
		public Task<DiscordMessage> GetReplyInteraction(CancellationToken token = default) => throw new NotImplementedException();
		public ComponentDialogueSession(MyDiscordClient client, DiscordInteraction interaction, DiscordMessage msg)
		{
			Client = client;
			Identifier = new DialogueCommandIdentifier(Interactive = new(interaction, msg));
			NextType = InteractionResponseType.UpdateMessage;
		}
		public ComponentDialogueSession(MyDiscordClient client, DiscordInteraction interaction)
		{
			Client = client;
			Identifier = new DialogueCommandIdentifier(Interactive = new(interaction));
			NextType = InteractionResponseType.ChannelMessageWithSource;
		}
	}
}
