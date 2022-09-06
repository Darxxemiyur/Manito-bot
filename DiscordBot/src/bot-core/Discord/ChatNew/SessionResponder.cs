using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Manito.Discord.Client;

namespace Manito.Discord.ChatNew
{
	public class SessionResponder
	{
		public InteractionResponseType LastType {
			get; private set;
		} = InteractionResponseType.Pong;

		public InteractiveInteraction Interactive {
			get;
		}

		public async Task SendMessage(UniversalMessageBuilder message)
		{
			switch (LastType)
			{
				case InteractionResponseType.Pong:
					LastType = InteractionResponseType.ChannelMessageWithSource;
					await Interactive.Interaction.CreateResponseAsync(LastType, message);
					break;
				case InteractionResponseType.ChannelMessageWithSource:
					LastType = InteractionResponseType.UpdateMessage;
					await Interactive.Interaction.CreateResponseAsync(LastType, message);
					break;
				case InteractionResponseType.UpdateMessage:
					await Interactive.Interaction.CreateResponseAsync(LastType, message);
					break;
				case InteractionResponseType.DeferredChannelMessageWithSource:
				case InteractionResponseType.DeferredMessageUpdate:
					await Interactive.Interaction.EditOriginalResponseAsync(message);
					break;
				default:
					LastType = LastType;
					break;
			}

		}
		private async Task CancelClickability()
		{
			//var msg = Interactive.Message

			//await SendMessage();
			LastType = InteractionResponseType.DeferredMessageUpdate;
		}
		private async Task RespondToAnInteraction()
		{
			switch (LastType)
			{
				case InteractionResponseType.Pong:
					LastType = InteractionResponseType.DeferredChannelMessageWithSource;
					break;
				case InteractionResponseType.DeferredChannelMessageWithSource:
				case InteractionResponseType.DeferredMessageUpdate:
					return;
				default:
					LastType = InteractionResponseType.DeferredMessageUpdate;
					break;
			}

			await Interactive.Interaction.CreateResponseAsync(LastType);
		}
		public async Task DoLaterReply()
		{
			if (Interactive.Components.Any())
				await CancelClickability();
			else
				await RespondToAnInteraction();
		}
	}
}
