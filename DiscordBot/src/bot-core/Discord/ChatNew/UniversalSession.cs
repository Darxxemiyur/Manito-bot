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
		public ISessionState Identifier => _innerSession.Identifier;

		public UniversalSession ToUniversal() => this;

		public UniversalSession(IDialogueSession session) => DoSub(_innerSession = session);

		private void DoSub(IDialogueSession session)
		{
			session.OnStatusChange += Session_OnStatusChange;
			session.OnSessionEnd += Session_OnSessionEnd;
			session.OnRemove += Session_OnRemove;
		}

		private async Task<bool> Session_OnRemove(IDialogueSession arg) => OnRemove != null && await OnRemove(this);

		private async Task Session_OnSessionEnd(IDialogueSession arg1, SessionInnerMessage arg2)
		{
			if (OnSessionEnd != null)
				await OnSessionEnd(this, arg2);
		}

		private async Task Session_OnStatusChange(IDialogueSession arg1, SessionInnerMessage arg2)
		{
			if (await ConvertSession(arg1, arg2))
				return;

			if (OnStatusChange != null)
				await OnStatusChange(this, arg2);
		}

		private void UnDoSub(IDialogueSession session)
		{
			session.OnStatusChange -= Session_OnStatusChange;
			session.OnSessionEnd -= Session_OnSessionEnd;
			session.OnRemove -= Session_OnRemove;
		}

		private async Task<bool> ConvertSession(IDialogueSession session, SessionInnerMessage msg)
		{
			if (!msg.Message.ToLower().Contains("convertme"))
				return true;

			if (msg.Message.ToLower().Contains("tocomp"))
			{
				var intr = (InteractiveInteraction)msg.Generic;
				_innerSession = new ComponentDialogueSession(Client, new DialogueCompInterIdentifier(Client, intr), intr);
			}
			if (msg.Message.ToLower().Contains("tomsg1"))
			{
				var (msgg, bld, id) = ((DiscordMessage, UniversalMessageBuilder, ulong))msg.Generic;
				_innerSession = new SessionFromMessage(Client, msgg, bld, id);
			}
			if (msg.Message.ToLower().Contains("tomsg2"))
			{
				var (chnl, bld, id) = ((DiscordChannel, UniversalMessageBuilder, ulong))msg.Generic;
				_innerSession = new SessionFromMessage(Client, chnl, bld, id);
			}

			UnDoSub(session);
			DoSub(_innerSession);
			await DoLaterReply();
			return false;
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
				try
				{
					for (var j = 0; j < v; j++)
					{
						try
						{
							await actor();
							return;
						}
						catch (Exception ev) when (j < v - 1)
						{
							await Client.Domain.Logging.WriteErrorClassedLog(GetType().Name, ev, true);
						}
					}
				}
				catch (Exception e) when (i * v < lim)
				{
					await Client.Domain.Logging.WriteErrorClassedLog(GetType().Name, e, true);
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

		public Task RemoveMessage()
		{
			return Client.Remover.RemoveMessage(Identifier.ChannelId, Identifier.MessageId ?? 0);
			//return SafeWriter(() => _innerSession.RemoveMessage());
		}

		public Task SendMessage(UniversalMessageBuilder msg) => SafeWriter(() => _innerSession.SendMessage(msg));

		public Task<DiscordMessage> SessionMessage => _innerSession.SessionMessage;

		public Task<DiscordChannel> SessionChannel => _innerSession.SessionChannel;
	}
}