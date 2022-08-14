using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Manito.Discord.Client;
using Name.Bayfaderix.Darxxemiyur.Common;
using Manito.Discord.Economy;
using Manito.Discord.Filters;
using Manito.Discord.Inventory;
using Manito.Discord.Shop;
using Manito.Discord.PermanentMessage;

namespace Manito.Discord.Client
{
    /// <summary>
    /// Holding class for event filters.
    /// </summary>
    public class EventFilters : IModule
    {
        private MyDomain _service;
        private EventBuffer _eventBuffer;
        private ShopFilter _shopFilter;
        private NoiseFilter _noiseFilter;
        private InventoryFilter _inventoryFilter;
        private EconomyFilter _economyFilter;
        private DebugFilter _debugFilter;
        private MsgWallFilter _msgWallFilter;
        public EventBuffer MyEventBuffer => _eventBuffer;
        public ShopFilter Shop => _shopFilter;
        public NoiseFilter Noise => _noiseFilter;
        public InventoryFilter InventoryFilter => _inventoryFilter;
        public EconomyFilter Economy => _economyFilter;
        public DebugFilter Debug => _debugFilter;
        public EventFilters(MyDomain service, EventBuffer eventBuffer)
        {
            _service = service;
            _eventBuffer = eventBuffer;
        }
        public async Task Initialize()
        {
            _msgWallFilter = new MsgWallFilter(_service, _eventBuffer);
            _noiseFilter = new NoiseFilter(_eventBuffer);
            _economyFilter = new EconomyFilter(_service, _eventBuffer);
            _inventoryFilter = new InventoryFilter(_service, _eventBuffer);
            _shopFilter = new ShopFilter(_service, _eventBuffer);
            _debugFilter = new DebugFilter(_service, _eventBuffer);
        }

        public async Task PostInitialize()
        {

        }
        private IEnumerable<Task> GetRuns()
        {
            yield return _inventoryFilter.RunModule();
            yield return _shopFilter.RunModule();
            yield return _economyFilter.RunModule();
            yield return _debugFilter.RunModule();
            yield return _msgWallFilter.RunModule();
        }
        public Task RunModule() => Task.WhenAll(GetRuns());
    }
}