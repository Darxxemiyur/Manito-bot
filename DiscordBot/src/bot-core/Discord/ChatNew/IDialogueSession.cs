using DisCatSharp.Entities;

using Manito.Discord.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Manito.Discord.ChatNew
{
	public interface IDialogueSession
	{
		/// <summary>
		/// My own discord client wrapper.
		/// </summary>
		MyClientBundle Client {
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

		UniversalSession ToUniversal();
	}
}