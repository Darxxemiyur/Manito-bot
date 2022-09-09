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
		}

		public InteractiveInteraction Interactive {
			get; private set;
		}
		private SessionInformation Information {
			get; set;
		}
		public SessionResponder(SessionInformation information, InteractiveInteraction interaction)
		{
			information.OnInteractionUpdate += UpdateInteractiveInteraction;
			Information = information;
			Interactive = interaction;
			LastType = InteractionResponseType.ChannelMessageWithSource;
		}
		private void UpdateInteractiveInteraction(object sender, InteractiveInteraction interaction)
		{
			Interactive = interaction;
			LastType = InteractionResponseType.UpdateMessage;
		}
		public async Task SendMessage(UniversalMessageBuilder message)
		{
			switch (LastType)
			{
				case InteractionResponseType.ChannelMessageWithSource:
					await Interactive.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, message);
					await UpdateIdentifier();
					LastType = InteractionResponseType.Pong;
					break;
				case InteractionResponseType.UpdateMessage:
					await Interactive.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, message);
					LastType = InteractionResponseType.Pong;
					break;
				case InteractionResponseType.Pong:
					await Interactive.Interaction.EditOriginalResponseAsync(message);
					break;
			}

		}
		private async Task UpdateIdentifier()
		{
			var msg = await Interactive.Interaction.GetOriginalResponseAsync();
			Information.UpdateId(new DialogueMessageIdentifier(new(Interactive.Interaction, msg)));
		}
		private async Task CancelClickability()
		{
			var builder = new UniversalMessageBuilder(Interactive.Message);

			var components = builder.Components.Select(x => x.Where(x => x is DiscordButtonComponent)
			.Select(x => ((DiscordButtonComponent)x).Disable()).ToArray()).ToArray();

			builder.SetComponents(components);

			await SendMessage(builder);
			LastType = InteractionResponseType.Pong;
		}
		private async Task RespondToAnInteraction()
		{
			switch (LastType)
			{
				case InteractionResponseType.ChannelMessageWithSource:
					await Interactive.Interaction
						.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
					LastType = InteractionResponseType.Pong;
					await UpdateIdentifier();
					break;
				case InteractionResponseType.UpdateMessage:
					await Interactive.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
					LastType = InteractionResponseType.Pong;
					break;
			}
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
