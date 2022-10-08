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
		public UniversalSession ToUniversal() => this;

		public UniversalSession(IDialogueSession session) => (_innerSession = session).OnStatusChange += ConvertSession;

		private async Task ConvertSession(IDialogueSession session, SessionInnerMessage msg)
		{
			if (!msg.Message.ToLower().Contains("convertme"))
				return;

			if (msg.Message.ToLower().Contains("tocomp"))
			{
				var (intr, iden, msgg) = ((DiscordInteraction, DialogueCompInterIdentifier, DiscordMessage))msg.Generic;
				_innerSession = new ComponentDialogueSession(Client, iden, new InteractiveInteraction(intr, msgg));
			}
			if (msg.Message.ToLower().Contains("tomsg1"))
			{
				var (msgg, id) = ((DiscordMessage, ulong))msg.Generic;
				_innerSession = new SessionFromMessage(Client, msgg, id);
			}
			if (msg.Message.ToLower().Contains("tomsg2"))
			{
				var (chnl, id) = ((DiscordChannel, ulong))msg.Generic;
				_innerSession = new SessionFromMessage(Client, chnl, id);
			}

			session.OnStatusChange -= ConvertSession;
			_innerSession.OnStatusChange += ConvertSession;
		}

		public event Func<IDialogueSession, SessionInnerMessage, Task> OnStatusChange;

		public event Func<IDialogueSession, SessionInnerMessage, Task> OnSessionEnd;

		public event Func<IDialogueSession, Task<bool>> OnRemove;

		private async Task SafeWriter(Func<Task> actor)
		{
			var v = 10;
			var lim = 720;
			var timeout = TimeSpan.FromSeconds(15000);
			for (var i = 0; ; i++)
			{
				for (var j = 0; j < v; j++)
				{
					try
					{
						await actor();
						return;
					}
					catch (Exception e) when (((i * v) + j) < lim)
					{
						await Client.Domain.Logging.WriteErrorClassedLog(GetType().Name, e, true);
					}
				}
				await Task.Delay(timeout);
			}
		}
		public Task DoLaterReply() => SafeWriter(() => _innerSession.DoLaterReply());
		public Task EndSession() => _innerSession.EndSession();


		public Task<InteractiveInteraction> GetComponentInteraction(CancellationToken token = default) => _innerSession.GetComponentInteraction(token);

		public Task<GeneralInteraction> GetInteraction(InteractionTypes types, CancellationToken token = default) => _innerSession.GetInteraction(types);

		public Task<DiscordMessage> GetMessageInteraction(CancellationToken token = default) => _innerSession.GetMessageInteraction(token);

		public Task<DiscordMessage> GetReplyInteraction(CancellationToken token = default) =>
			_innerSession.GetReplyInteraction(token);

		public Task RemoveMessage() => SafeWriter(() => _innerSession.RemoveMessage());

		public Task SendMessage(UniversalMessageBuilder msg) => SafeWriter(() => _innerSession.SendMessage(msg));

		public Task<DiscordMessage> SessionMessage => _innerSession.SessionMessage;

		public Task<DiscordChannel> SessionChannel => _innerSession.SessionChannel;
	}
}