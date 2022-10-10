﻿using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;

using Manito.Discord.Client;

using Name.Bayfaderix.Darxxemiyur.Common;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Manito.Discord.ChatNew
{
	public class SessionFromMessage : IDialogueSession
	{
		private DiscordMessage _message;
		private DiscordChannel _channel;
		private ulong _userId;
		private UniversalMessageBuilder _builder;

		public bool IsAutomaticallyDeleted {
			get;
		}

		public SessionFromMessage(MyClientBundle client, DiscordMessage message, UniversalMessageBuilder bld, ulong userId, bool isAutomaticallyDeleted = true) : this(userId, client, isAutomaticallyDeleted)
		{
			Identifier = new DialogueMsgIdentifier(_message = message, _userId = userId);
			_builder = bld;
			_channel = message.Channel;
		}

		public SessionFromMessage(MyClientBundle client, DiscordChannel channel, UniversalMessageBuilder bld, ulong userId, bool isAutomaticallyDeleted = true) : this(userId, client, isAutomaticallyDeleted)
		{
			_channel = channel;
			_builder = bld;
		}

		public SessionFromMessage(MyClientBundle client, DiscordMessage message, ulong userId, bool isAutomaticallyDeleted = true) : this(userId, client, isAutomaticallyDeleted)
		{
			Identifier = new DialogueMsgIdentifier(_message = message, userId);
			_channel = message.Channel;
		}

		public SessionFromMessage(MyClientBundle client, DiscordChannel channel, ulong userId, bool isAutomaticallyDeleted = true) : this(userId, client, isAutomaticallyDeleted)
		{
			Identifier = new NoMessageDiagIdentifier(channel.Id, userId);
			_channel = channel;
		}

		private SessionFromMessage(ulong userId, MyClientBundle client, bool isAutomaticallyDeleted)
		{
			IsAutomaticallyDeleted = isAutomaticallyDeleted;
			_userId = userId;
			Client = client;
			Client.EventsBuffer.MsgDeletion.OnToNextLink += MsgDeletion_OnMessage;
		}

		~SessionFromMessage()
		{
			Client.EventsBuffer.MsgDeletion.OnToNextLink -= MsgDeletion_OnMessage;
		}

		private async Task MsgDeletion_OnMessage(DiscordClient arg1, MessageDeleteEventArgs arg2)
		{
			if (arg2.Message.Id != Identifier.MessageId)
				return;

			await Client.Domain.ExecutionThread.AddNew(new ExecThread.Job(EndSession));
		}

		public UniversalSession ToUniversal() => (UniversalSession)this;

		public MyClientBundle Client {
			get;
		}

		public ISessionState Identifier {
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

			await OnStatusChange(this, new(new InteractiveInteraction(intr.Interaction, _message), "ConvertMeToComp"));

			return intr;
		}

		public Task<DiscordMessage> GetReplyInteraction(CancellationToken token = default)
		{
			throw new NotImplementedException();
		}

		private AsyncLocker _lock = new();

		public async Task SendMessage(UniversalMessageBuilder msg)
		{
			await using var _ = await _lock.BlockAsyncLock();
			await SendMessageLocal(msg);
			await Client.Remover.RemoveMessage(Identifier.ChannelId, Identifier.MessageId ?? 0, true);
		}

		private async Task SendMessageLocal(UniversalMessageBuilder msg)
		{
			if (_message != null)
			{
				await _message.ModifyAsync(msg);
				return;
			}

			_message = await _channel.SendMessageAsync(msg);
			Identifier = new DialogueMsgIdentifier(_message, _userId);
		}

		public async Task DoLaterReply()
		{
			await using var _ = await _lock.BlockAsyncLock();
			if (_builder?.Components?.SelectMany(x => x)?.Any() != true)
				return;

			var msg = _builder;
			await SendMessageLocal(msg.NewWithDisabledComponents());
			await Client.Remover.RemoveMessage(Identifier.ChannelId, Identifier.MessageId ?? 0, true);
			_builder = msg;
		}

		public async Task RemoveMessage()
		{
			await using var _ = await _lock.BlockAsyncLock();
			await Client.Remover.RemoveMessage(Identifier.ChannelId, Identifier.MessageId ?? 0);
		}

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