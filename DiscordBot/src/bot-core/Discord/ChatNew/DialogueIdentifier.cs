using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Manito.Discord.Client;

using System;

namespace Manito.Discord.ChatNew
{
	/// <summary>
	/// Secondary Message Identifier
	/// </summary>
	public class DialogueMsgIdentifier : IDialogueIdentifier
	{
		public DialogueMsgIdentifier(DiscordMessage message, ulong userId)
		{
			_usId = userId;
			_chId = message.ChannelId;
			_msId = message.Id;
		}

		private ulong _usId;
		private ulong _chId;
		private ulong _msId;
		public ulong UserId => _usId;
		public ulong ChannelId => _chId;
		public ulong MessageId => _msId;

		public bool DoesBelongToUs(InteractiveInteraction interaction)
		{
			var mid = interaction.Message.Id;
			var cid = interaction.Message.ChannelId;
			var uid = interaction.Interaction.User.Id;

			return _chId == cid && mid == _msId && uid == _usId;
		}

		public Int32 HowBadWants(InteractiveInteraction interaction) => 10;

		public bool DoesBelongToUs(DiscordMessage interaction)
		{
			var cid = interaction.ChannelId;
			var uid = interaction.Author.Id;

			return _chId == cid && uid == _usId;
		}

		public Int32 HowBadWants(DiscordMessage interaction) => 10;
	}

	public class DialogueMessageIdentifier : IDialogueIdentifier
	{
		public DialogueMessageIdentifier(InteractiveInteraction interaction)
		{
			_usId = interaction.Interaction.User.Id;
			_chId = interaction.Interaction.ChannelId;
			_msId = interaction.Message.Id;
		}

		private ulong _usId;
		private ulong _chId;
		private ulong _msId;
		public ulong UserId => _usId;
		public ulong ChannelId => _chId;
		public ulong MessageId => _msId;

		public bool DoesBelongToUs(InteractiveInteraction interaction)
		{
			var mid = interaction.Message.Id;
			var cid = interaction.Message.ChannelId;
			var uid = interaction.Interaction.User.Id;

			return _chId == cid && mid == _msId && uid == _usId;
		}

		public Int32 HowBadWants(InteractiveInteraction interaction) => 10;

		public bool DoesBelongToUs(DiscordMessage interaction)
		{
			var cid = interaction.ChannelId;
			var uid = interaction.Author.Id;

			return _chId == cid && uid == _usId;
		}

		public Int32 HowBadWants(DiscordMessage interaction) => 10;
	}

	public class DialogueCompInterIdentifier : IDialogueIdentifier
	{
		public DialogueCompInterIdentifier(InteractiveInteraction interaction)
		{
			_usId = interaction.Interaction.User.Id;
			_chId = interaction.Interaction.ChannelId;
			_msId = interaction.Message.Id;
		}

		private ulong _usId;
		private ulong _chId;
		private ulong _msId;
		public ulong UserId => _usId;
		public ulong ChannelId => _chId;
		public ulong MessageId => _msId;

		public bool DoesBelongToUs(InteractiveInteraction interaction)
		{
			var mid = interaction.Message.Id;
			var cid = interaction.Interaction.ChannelId;
			var uid = interaction.Interaction.User.Id;

			return _chId == cid && mid == _msId && uid == _usId && interaction.Interaction.Type == InteractionType.Component;
		}

		public Int32 HowBadWants(InteractiveInteraction interaction) => 100;

		public bool DoesBelongToUs(DiscordMessage interaction)
		{
			var cid = interaction.ChannelId;
			var uid = interaction.Author.Id;

			return _chId == cid && uid == _usId;
		}

		public Int32 HowBadWants(DiscordMessage interaction) => 100;
	}

	public class DialogueCommandIdentifier : IDialogueIdentifier
	{
		public ulong UserId {
			get;
		}

		public ulong ChannelId {
			get;
		}

		public ulong MessageId => 0;

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