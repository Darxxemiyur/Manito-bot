using DisCatSharp.Entities;

using Manito.Discord.Client;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Manito.Discord.ChatNew
{
	public class SessionFromMessage : IDialogueSession
	{
		private DiscordMessage _message;
		private DiscordChannel _channel;
		private ulong _userId;

		public SessionFromMessage(MyClientBundle client, DiscordMessage message, ulong userId)
		{
			Client = client;
			Identifier = new DialogueMsgIdentifier(_message = message, _userId = userId);
			_channel = message.Channel;
		}

		public SessionFromMessage(MyClientBundle client, DiscordChannel channel, ulong userId)
		{
			Client = client;
			_channel = channel;
			_userId = userId;
		}

		public UniversalSession ToUniversal() => (UniversalSession)this;

		public MyClientBundle Client {
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

			await OnStatusChange(this, new SessionInnerMessage((intr.Interaction, new DialogueCompInterIdentifier(intr), _message), "ConvertMeToComp"));

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

		public static implicit operator UniversalSession(SessionFromMessage msg) => new(msg);
	}
}