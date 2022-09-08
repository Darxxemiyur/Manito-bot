using DSharpPlus.Entities;

using Manito.Discord.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Manito.Discord.ChatNew
{
	public class InteractionPuller
	{

		/// <summary>
		/// Identifier of session to pull events for.
		/// </summary>
		public MyDiscordClient Client {
			get; private set;
		}
		public IDialogueIdentifier Identifier {
			get; private set;
		}
		public InteractionPuller(MyDiscordClient client)
		{
			Client = client;
		}

		public async Task<DiscordMessage> GetMessageInteraction(CancellationToken token = default)
		{
			throw new NotImplementedException();

			//var msg = await Client.ActivityTools.WaitForMessage(x => Identifier.DoesBelongToUs(x), token);

			//return msg.;
		}
		public async Task<InteractiveInteraction> GetComponentInteraction(CancellationToken token = default)
		{
			var intr = await Client.ActivityTools.WaitForComponentInteraction(x => Identifier.DoesBelongToUs(x), token);

			return intr;
		}
		public async Task<GeneralInteraction> GetInteraction(InteractionTypes types)
		{
			CancellationTokenSource cancellation = new();

			var tasks = new List<(InteractionTypes, Task)>();
			if (types.HasFlag(InteractionTypes.Component))
			{
				tasks.Add((InteractionTypes.Component, GetComponentInteraction(cancellation.Token)));
			}
			if (types.HasFlag(InteractionTypes.Message))
			{
				tasks.Add((InteractionTypes.Message, GetMessageInteraction(cancellation.Token)));
			}

			var first = await Task.WhenAny(tasks.Select(x => x.Item2));

			cancellation.Cancel();

			var couple = tasks.First(x => x.Item2 == first);



			return new GeneralInteraction(couple.Item1,
				couple.Item2 is Task<InteractiveInteraction> i ? await i : null,
				couple.Item2 is Task<DiscordMessage> m ? await m : null);
		}
	}
}
