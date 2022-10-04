using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.EventArgs;
using DisCatSharp.ApplicationCommands.Attributes;

using Manito.Discord.Client;
using Name.Bayfaderix.Darxxemiyur.Common;
using Manito.System.Economy; using Manito.Discord;
using System.Threading;

namespace Manito.Discord.Shop
{

	public class ShopFilter : IModule
	{
		public Task RunModule() => Task.WhenAll(HandleLoop(), RunHooked());
		private async Task HandleLoop()
		{
			while (true)
			{
				var data = (await _queue.GetData()).Item2;
				await HandleAsCommand(data);
			}
		}
		private async Task<ulong> CheckMessage(ulong chnlId, ulong msgId)
		{
			var clnt = _service.MyDiscordClient.Client;
			var chnl = await clnt.GetChannelAsync(chnlId);

			try
			{
				var msg = await chnl.GetMessageAsync(msgId);
			}
			catch (DisCatSharp.Exceptions.NotFoundException)
			{
				return (await chnl.SendMessageAsync(GetMsg())).Id;
			}

			return msgId;
		}
		private DiscordMessageBuilder GetMsg() => _shopService.GetEnterMessage();
		private async Task RunHooked()
		{
			ulong chnlId = 965561900517716008;
			ulong messageId = 966798532990345276;
			while (true)
			{
				messageId = await CheckMessage(chnlId, messageId);
				var inter = await _service.MyDiscordClient.ActivityTools
					.WaitForComponentInteraction(x => x.Message.Id == messageId);

				var payload = inter.Interaction;
				await _queue.Handle(_service.MyDiscordClient.Client, payload);
			}
		}
		private ShopService _shopService;
		private MyDomain _service;
		private List<DiscordApplicationCommand> _commandList;
		private DiscordEventProxy<DiscordInteraction> _queue;
		public ShopFilter(MyDomain service, EventBuffer eventBuffer)
		{
			_service = service;
			_shopService = service.ShopService;
			_commandList = GetCommands().ToList();
			service.MyDiscordClient.AppCommands.Add("Shop", _commandList);
			_queue = new();
			eventBuffer.Interact.OnMessage += FilterMessage;
		}
		private IEnumerable<DiscordApplicationCommand> GetCommands()
		{
			yield return new DiscordApplicationCommand("shopping",
			 "Начать шоппинг");
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
			var res = _shopService.StartSession(args.User, args);

			if (res == null)
			{
				await args.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
					new DiscordInteractionResponseBuilder().AddEmbed(_shopService.Default()
					.WithDescription("Вы уже открыли магазин!")).AsEphemeral(true));
			}
		}
	}

}