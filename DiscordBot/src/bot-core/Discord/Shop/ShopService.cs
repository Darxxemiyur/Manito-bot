using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using Manito.Discord.Database;
using System.Linq;
using Manito.Discord.Client;
using Name.Bayfaderix.Darxxemiyur.Common;
using System.Threading;
using Manito.Discord.Chat.DialogueNet;
using Manito.Discord.ChatNew;
using Manito.Discord.ChatAbstract;

namespace Manito.Discord.Shop
{

	public class ShopService
	{
		private MyDomain _service;
		private MyDiscordClient _client;
		private ShopCashRegister _cashRegister;
		private DialogueNetSessionTab<ShopContext> _shopTab;
		private SemaphoreSlim _lock;
		public ShopService(MyDomain service)
		{
			_lock = new SemaphoreSlim(1, 1);
			_service = service;
			_client = service.MyDiscordClient;
			_shopTab = new(service);
			_cashRegister = new(null);
		}
		public async Task<DialogueTabSession<ShopContext>> StartSession(DiscordUser customer, DiscordInteraction intr)
		{
			await _lock.WaitAsync();

			DialogueTabSession<ShopContext> session = null;

			if (_shopTab.Sessions.All(x => x.Context.CustomerId != customer.Id))
				session = await _shopTab.CreateSession(new(intr), new(customer.Id,
				new Economy.PlayerWallet(_service.Economy, customer.Id), _cashRegister, this),
				(x) => Task.FromResult((IDialogueNet)new ShopDialogue(x)));

			_lock.Release();

			return session;
		}
		public DiscordEmbedBuilder Default(DiscordEmbedBuilder bld = null) =>
			_cashRegister.Default(bld);

		public DiscordMessageBuilder GetEnterMessage()
		{
			return new DiscordMessageBuilder().AddEmbed(Default().WithDescription("{}"))
				.AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "Start", "Шоппинг!"));
		}
	}

}
