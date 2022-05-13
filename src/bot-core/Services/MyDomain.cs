using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Manito.Discord.Client;
using Manito.Discord.Economy;
using Manito.Discord.Shop;

namespace Manito.Discord
{
    public class MyDomain
    {
        private ApplicationCommands _appCommands;
        private MyDiscordClient _myDiscordClient;
        private EventFilters _filters;
        private ExecThread _executionThread;
        private ServerEconomy _economy;
        private ShopService _shopService;
        public MyDiscordClient MyDiscordClient => _myDiscordClient;
        public ExecThread ExecutionThread => _executionThread;
        public ServerEconomy Economy => _economy;
        public ShopService ShopService => _shopService;
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
            _economy = new(this);
            _myDiscordClient = new MyDiscordClient(this);
            _shopService = new ShopService(this);
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
        }
    }
}