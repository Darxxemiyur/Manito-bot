using System;
using System.Threading.Tasks;

using Manito.Discord.Client;
using Manito.Discord.Economy;

namespace Manito.Discord
{
    public class MyService
    {
        private ApplicationCommands _appCommands;
        private MyDiscordClient _myDiscordClient;
        public MyDiscordClient MyDiscordClient => _myDiscordClient;
        private EventFilters _filters;
        public EventFilters Filters => _filters;
        public static async Task<MyService> Create()
        {
            var service = new MyService();

            await service.Initialize();

            return service;
        }
        private MyService()
        {

        }
        private async Task Initialize()
        {

            _myDiscordClient = new MyDiscordClient(this);
            _appCommands = _myDiscordClient.AppCommands;
            _filters = new EventFilters(this, _myDiscordClient.EventsBuffer);
            await _filters.Initialize();
        }

        public async Task StartBot()
        {
            await _myDiscordClient.Start();
            await _filters.PostInitialize();
            await _appCommands.UpdateCommands();
            await _myDiscordClient.StartLongTerm();

        }
    }
}