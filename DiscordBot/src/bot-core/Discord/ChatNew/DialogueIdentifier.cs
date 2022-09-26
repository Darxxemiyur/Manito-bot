using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Manito.Discord.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Manito.Discord.ChatNew
{
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
		public Boolean DoesBelongToUs(InteractiveInteraction interaction)
		{
			var mid = interaction.Message.Id;
			var cid = interaction.Message.ChannelId;
			var uid = interaction.Interaction.User.Id;

			return _chId == cid && mid == _msId && uid == _usId;
		}
		public Int32 HowBadWants(InteractiveInteraction interaction) => 10;
		public Boolean DoesBelongToUs(DiscordMessage interaction)
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
		public Boolean DoesBelongToUs(InteractiveInteraction interaction)
		{
			var mid = interaction.Message.Id;
			var cid = interaction.Interaction.ChannelId;
			var uid = interaction.Interaction.User.Id;

			return _chId == cid && mid == _msId && uid == _usId && interaction.Interaction.Type == InteractionType.Component;
		}
		public Int32 HowBadWants(InteractiveInteraction interaction) => 100;
		public Boolean DoesBelongToUs(DiscordMessage interaction)
		{
			var cid = interaction.ChannelId;
			var uid = interaction.Author.Id;

			return _chId == cid && uid == _usId;
		}
		public Int32 HowBadWants(DiscordMessage interaction) => 100;
	}
	public class DialogueCommandIdentifier : IDialogueIdentifier
	{
		public DialogueCommandIdentifier(InteractiveInteraction interaction)
		{
			_usId = interaction.Interaction.User.Id;
			_chId = interaction.Interaction.ChannelId;
		}
		private ulong _usId;
		private ulong _chId;
		private ulong _msId;
		public Boolean DoesBelongToUs(InteractiveInteraction interaction)
		{
			var mid = interaction.Message.Id;
			var cid = interaction.Interaction.ChannelId;
			var uid = interaction.Interaction.User.Id;

			return _chId == cid && mid == _msId && uid == _usId;
		}
		public Int32 HowBadWants(InteractiveInteraction interaction) => 100;
		public Boolean DoesBelongToUs(DiscordMessage interaction)
		{
			var cid = interaction.ChannelId;
			var uid = interaction.Author.Id;

			return _chId == cid && uid == _usId;
		}
		public Int32 HowBadWants(DiscordMessage interaction) => 100;
	}
}
