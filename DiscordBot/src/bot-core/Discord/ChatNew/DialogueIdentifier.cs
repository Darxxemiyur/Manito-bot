using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Net;

using Manito.Discord.Client;

using System;

namespace Manito.Discord.ChatNew
{
	/// <summary>
	/// Secondary Message Identifier
	/// </summary>
	public class DialogueMsgIdentifier : ISessionState
	{
		public DialogueMsgIdentifier(DiscordMessage message, ulong userId)
		{
			UserId = userId;
			ChannelId = message.ChannelId;
			MessageId = message.Id;
		}

		public ulong? UserId {
			get;
		}

		public ulong ChannelId {
			get;
		}

		public ulong? MessageId {
			get;
		}

		public ulong[] UserIds => new[]
		{
			UserId ?? 0
		};

		public SessionKinds Kind => SessionKinds.OnDMChannel | SessionKinds.OnGuildChannel;

		public MyClientBundle Bundle {
			get;
		}

		public DiscordApiClient UsedClient {
			get;
		}

		public bool DoesBelongToUs(InteractiveInteraction interaction)
		{
			var mid = interaction.Message.Id;
			var cid = interaction.Message.ChannelId;
			var uid = interaction.Interaction.User.Id;

			return ChannelId == cid && mid == MessageId && uid == UserId;
		}

		public Int32 HowBadWants(InteractiveInteraction interaction) => 10;

		public bool DoesBelongToUs(DiscordMessage interaction)
		{
			var cid = interaction.ChannelId;
			var uid = interaction.Author.Id;

			return ChannelId == cid && uid == UserId;
		}

		public Int32 HowBadWants(DiscordMessage interaction) => 10;
	}

	public class DialogueMessageIdentifier : ISessionState
	{
		public DialogueMessageIdentifier(InteractiveInteraction interaction)
		{
			UserId = interaction.Interaction.User.Id;
			ChannelId = interaction.Interaction.ChannelId;
			MessageId = interaction.Message.Id;
		}

		public ulong? UserId {
			get;
		}

		public ulong ChannelId {
			get;
		}

		public ulong? MessageId {
			get;
		}

		public ulong[] UserIds => new[]
		{
			UserId ?? 0
		};

		public SessionKinds Kind => SessionKinds.OnDMChannel | SessionKinds.OnGuildChannel;

		public MyClientBundle Bundle {
			get;
		}

		public DiscordApiClient UsedClient {
			get;
		}

		public bool DoesBelongToUs(InteractiveInteraction interaction)
		{
			var mid = interaction.Message.Id;
			var cid = interaction.Message.ChannelId;
			var uid = interaction.Interaction.User.Id;

			return ChannelId == cid && mid == MessageId && uid == UserId;
		}

		public Int32 HowBadWants(InteractiveInteraction interaction) => 10;

		public bool DoesBelongToUs(DiscordMessage interaction)
		{
			var cid = interaction.ChannelId;
			var uid = interaction.Author.Id;

			return ChannelId == cid && uid == UserId;
		}

		public Int32 HowBadWants(DiscordMessage interaction) => 10;
	}

	public class DialogueCompInterIdentifier : ISessionState
	{
		public DialogueCompInterIdentifier(MyClientBundle bundle, InteractiveInteraction interaction)
		{
			UserId = interaction.Interaction.User.Id;
			ChannelId = interaction.Interaction.ChannelId;
			MessageId = interaction.Message.Id;
		}

		public ulong? UserId {
			get;
		}

		public ulong ChannelId {
			get;
		}

		public ulong? MessageId {
			get;
		}

		public ulong[] UserIds {
			get;
		}

		public SessionKinds Kind => SessionKinds.OnDMChannel | SessionKinds.OnGuildChannel;

		public MyClientBundle Bundle {
			get;
		}

		public DiscordApiClient UsedClient {
			get;
		}

		public bool DoesBelongToUs(InteractiveInteraction interaction)
		{
			var mid = interaction.Message.Id;
			var cid = interaction.Interaction.ChannelId;
			var uid = interaction.Interaction.User.Id;

			return ChannelId == cid && mid == MessageId && uid == UserId && interaction.Interaction.Type == InteractionType.Component;
		}

		public Int32 HowBadWants(InteractiveInteraction interaction) => 100;

		public bool DoesBelongToUs(DiscordMessage interaction)
		{
			var cid = interaction.ChannelId;
			var uid = interaction.Author.Id;

			return ChannelId == cid && uid == UserId;
		}

		public Int32 HowBadWants(DiscordMessage interaction) => 100;
	}

	public class DialogueCommandIdentifier : ISessionState
	{
		public ulong? UserId {
			get;
		}

		public ulong ChannelId {
			get;
		}

		public ulong? MessageId => null;

		public ulong[] UserIds => new[]
		{
			UserId ?? 0
		};

		public SessionKinds Kind => SessionKinds.OnDMChannel | SessionKinds.OnGuildChannel;

		public MyClientBundle Bundle {
			get;
		}

		public DiscordApiClient UsedClient {
			get;
		}

		public DialogueCommandIdentifier(InteractiveInteraction interaction)
		{
			UserId = interaction.Interaction.User.Id;
			ChannelId = interaction.Interaction.ChannelId;
		}

		public bool DoesBelongToUs(InteractiveInteraction interaction) => false;

		public Int32 HowBadWants(InteractiveInteraction interaction) => 100;

		public bool DoesBelongToUs(DiscordMessage interaction) => false;

		public Int32 HowBadWants(DiscordMessage interaction) => 100;
	}
}