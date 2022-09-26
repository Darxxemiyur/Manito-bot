using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Manito.Discord.Client;

namespace Manito.Discord.ChatNew
{
	public class SessionResponder
	{
		public InteractionResponseType NextType {
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
			NextType = InteractionResponseType.ChannelMessageWithSource;
		}
		private void UpdateInteractiveInteraction(object sender, InteractiveInteraction interaction)
		{
			Interactive = interaction;
			NextType = InteractionResponseType.UpdateMessage;
		}
		public async Task SendMessage(UniversalMessageBuilder message)
		{
			switch (NextType)
			{
				case InteractionResponseType.ChannelMessageWithSource:
					await Interactive.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, message);
					NextType = InteractionResponseType.Pong;
					break;
				case InteractionResponseType.UpdateMessage:
					await Interactive.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, message);
					NextType = InteractionResponseType.Pong;
					break;
				case InteractionResponseType.Pong:
					await Interactive.Interaction.EditOriginalResponseAsync(message);
					break;
			}
			await UpdateIdentifier();
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
			.Select(x => new DiscordButtonComponent((DiscordButtonComponent)x).Disable()).ToArray()).ToArray();

			builder.SetComponents(components);

			await SendMessage(builder);
			NextType = InteractionResponseType.Pong;
		}
		private async Task RespondToAnInteraction()
		{
			switch (NextType)
			{
				case InteractionResponseType.ChannelMessageWithSource:
					await Interactive.Interaction
						.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
					NextType = InteractionResponseType.Pong;
					break;
				case InteractionResponseType.UpdateMessage:
					await Interactive.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
					NextType = InteractionResponseType.Pong;
					break;
			}
			await UpdateIdentifier();
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
