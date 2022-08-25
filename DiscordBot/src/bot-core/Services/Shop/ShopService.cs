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

namespace Manito.Discord.Shop
{

	public class ShopService : DialogueNetSessionControls<ShopSession>
	{
		private MyDomain _service;
		private MyDiscordClient _client;
		private IShopDb _myDb;
		private ShopCashRegister _cashRegister;
		private List<ShopSession> _shopSessions;
		private SemaphoreSlim _lock;
		public ShopService(MyDomain service) : base(service)
		{
			_myDb = null;
			_service = service;
			_client = service.MyDiscordClient;
			_cashRegister = new(_myDb);
			_lock = new(1, 1);
		}
		public bool SessionExists(DiscordUser customer) => SessionExists(x => x.Customer == customer);

		public async Task<T> Atomary<T>(Func<ShopService, Task<T>> run)
		{
			await _lock.WaitAsync();
			var res = await run(this);
			_lock.Release();
			return res;
		}
		public async Task<ShopSession> StartSession(DiscordUser customer,
			DiscordInteraction intr)
		{
			var sess = new ShopSession(new(intr), _client, customer, _service.Economy.GetPlayerWallet(customer),
			 _service.Inventory.GetPlayerInventory(customer), _cashRegister);
			await StartSession(() => sess, x => x);

			return sess;
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
