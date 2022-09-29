using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp;
using Manito.Discord.Client;
using Manito.Discord.Shop;
using Manito.Discord;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Manito.Discord.ChatAbstract;
using Manito.Discord.Chat.DialogueNet;

namespace Manito.Discord.Orders
{
	public class AdminOrdersFilter : IModule
	{
		public Task RunModule() => HandleLoop();
		private async Task HandleLoop()
		{
			while (true)
			{
				var data = (await _queue.GetData()).Item2;
				await HandleAsCommand(data);
			}
		}
		private DialogueNetSessionTab<AdminOrderContext> _aoTab;
		private MyDomain _service;
		private List<DiscordApplicationCommand> _commandList;
		private DiscordEventProxy<DiscordInteraction> _queue;
		public AdminOrdersFilter(MyDomain service, EventBuffer eventBuffer)
		{
			_service = service;
			_aoTab = new(service);
			_commandList = GetCommands().ToList();
			_queue = new();
			service.MyDiscordClient.AppCommands.Add("AdmOrdFlt", _commandList);
			eventBuffer.Interact.OnMessage += FilterMessage;
		}
		private IEnumerable<DiscordApplicationCommand> GetCommands()
		{
			yield return new DiscordApplicationCommand("admin",
			 "Начать администрировать");
		}

		private async Task FilterMessage(DiscordClient client, InteractionCreateEventArgs args)
		{
			if (!_commandList.Any(x => args.Interaction.Data.Name.Contains(x.Name)))
				return;

			await _queue.Handle(client, args.Interaction);
			args.Handled = true;
		}
		private async Task HandleAsCommand(DiscordInteraction args)
		{
			var res = _aoTab.CreateSession(new(args), new(), x => Task.FromResult((IDialogueNet)new AdminOrderControl(x)));


		}
	}
}
