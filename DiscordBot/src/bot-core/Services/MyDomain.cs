using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Manito.Discord.Client;
using Name.Bayfaderix.Darxxemiyur.Common;
using Manito.Discord.Economy;
using Manito.Discord.Inventory;
using Manito.Discord.Shop;
using Microsoft.Extensions.DependencyInjection;
using Manito.Discord.PermanentMessage;
using Manito.Discord.Database;
using Manito.Discord.Config;
using Manito.Discord.Welcommer;

namespace Manito.Discord
{
	public class MyDomain
	{
		private IServiceCollection _serviceCollection;
		private MyDbFactory _db;
		public MyDbFactory DbFactory => _db;
		private ApplicationCommands _appCommands;
		private MyDiscordClient _myDiscordClient;
		private EventFilters _filters;
		private ExecThread _executionThread;
		private ServerEconomy _economy;
		private IInventorySystem _inventory;
		private WelcomerFilter _welcomer;
		private ShopService _shopService;
		private RootConfig _rootConfig;
		private MessageController _msgWallCtr;
		public IServiceCollection ServiceCollection => _serviceCollection;
		public MyDiscordClient MyDiscordClient => _myDiscordClient;
		public ExecThread ExecutionThread => _executionThread;
		public ServerEconomy Economy => _economy;
		public IInventorySystem Inventory => _inventory;
		public WelcomerFilter Welcomer => _welcomer;
		public ShopService ShopService => _shopService;
		public MessageController MsgWallCtr => _msgWallCtr;
		public static async Task<MyDomain> Create()
		{
			var service = new MyDomain();

			await service.Initialize();

			return service;
		}
		private MyDomain()
		{
		}
		private async Task Initialize()
		{
			_rootConfig = RootConfig.GetConfig();

			_db = new(this, _rootConfig.DatabaseCfg);
			_inventory = new TestInventorySystem();
			_economy = new(this, _db);
			_myDiscordClient = new MyDiscordClient(this, _rootConfig);
			_msgWallCtr = new(this);
			_shopService = new ShopService(this);
			_welcomer = new WelcomerFilter(_myDiscordClient);
			_appCommands = _myDiscordClient.AppCommands;
			_executionThread = new ExecThread();
			_filters = new EventFilters(this, _myDiscordClient.EventsBuffer);
			await _filters.Initialize();
		}

		public async Task StartBot()
		{
			await _appCommands.UpdateCommands();
			await _myDiscordClient.Start();
			await _filters.PostInitialize();
			await Task.WhenAll(GetTasks());

		}
		private IEnumerable<Task> GetTasks()
		{
			yield return _executionThread.Run();
			yield return _myDiscordClient.StartLongTerm();
			yield return _filters.RunModule();
			yield return _economy.RunModule();
			yield return _msgWallCtr.RunModule();
			yield return _welcomer.RunModule();
		}
	}
}