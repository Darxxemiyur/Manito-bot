using DisCatSharp.Entities;

using Manito.Discord.Client;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Manito.Discord.ChatNew
{
	public class UniversalSession : IDialogueSession
	{
		private IDialogueSession _innerSession;
		public MyClientBundle Client => _innerSession.Client;
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
}