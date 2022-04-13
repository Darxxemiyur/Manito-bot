using System;
using System.Threading.Tasks;

using Manito.Discord.Client;
using Manito.Discord.Economy;
using Manito.Discord.Filters;
using Manito.Discord.Shop;

namespace Manito.Discord.Client
{
    /// <summary>
    /// Holding class for event filters.
    /// </summary>
    public class EventFilters
    {
        private MyService _service;
        private EventBuffer _eventBuffer;
        public EventBuffer MyEventBuffer => _eventBuffer;
        private ShopFilter _shopFilter;
        public ShopFilter Shop => _shopFilter;
        private NoiseFilter _noiseFilter;
        public NoiseFilter Noise => _noiseFilter;
        private EconomyFilter _economyFilter;
        public EconomyFilter Economy => _economyFilter;
        public EventFilters(MyService service, EventBuffer eventBuffer)
        {
            _service = service;
            _eventBuffer = eventBuffer;
        }
        public async Task Initialize()
        {
            _noiseFilter = new NoiseFilter(_eventBuffer);
            _economyFilter = new EconomyFilter(_service, _eventBuffer);
            _shopFilter = new ShopFilter(_service, _eventBuffer);
        }

        public async Task PostInitialize()
        {


        }
    }
}