using DisCatSharp.Entities;

using Manito.Discord.Client;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Manito.Discord.ChatNew
{
	/// <summary>
	/// Session that relies on webhooks.
	/// </summary>
	public class WebhookMessageSession : IDialogueSession
	{
		public MyClientBundle Client {
			get;
		}

		public ISessionState Identifier {
			get;
		}

		public Task<DiscordMessage> SessionMessage {
			get;
		}

		public Task<DiscordChannel> SessionChannel {
			get;
		}

		private DiscordWebhook _client;

		public event Func<IDialogueSession, SessionInnerMessage, Task> OnStatusChange;

		public event Func<IDialogueSession, SessionInnerMessage, Task> OnSessionEnd;

		public event Func<IDialogueSession, Task<bool>> OnRemove;

		public async Task SendMessage(UniversalMessageBuilder msg)
		{
			//_client = Client.Client.Create
			throw new NotImplementedException();
		}

		public Task DoLaterReply() => throw new NotImplementedException();

		public Task EndSession() => throw new NotImplementedException();

		public Task<InteractiveInteraction> GetComponentInteraction(CancellationToken token = default) => throw new NotImplementedException();

		public Task<DiscordMessage> GetMessageInteraction(CancellationToken token = default) => throw new NotImplementedException();

		public Task<DiscordMessage> GetReplyInteraction(CancellationToken token = default) => throw new NotImplementedException();

		public Task RemoveMessage() => throw new NotImplementedException();

		public UniversalSession ToUniversal() => throw new NotImplementedException();
	}
}