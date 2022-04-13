using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using Manito.Discord.Db;
using System.Linq;
using Manito.Discord.Client;

namespace Manito.Discord.Shop
{

    public class ShopService
    {
        private MyDiscordClient _client;
        private IShopDb _myDb;
        private ShopCashRegister _cashRegister;
        private List<ShopSession> _shopSessions;
        public ShopService(MyService service)
        {
            _myDb = null;
            _client = service.MyDiscordClient;
            _cashRegister = new ShopCashRegister(_myDb);
            _shopSessions = new();
        }
        public bool CreateSession(DiscordUser customer)
        {
            if (_shopSessions.Any(x => x.Customer == customer))
                return false;

            _shopSessions.Add(new ShopSession(_client, customer, _cashRegister));
            return true;
        }
        public ShopSession GetSession(DiscordUser customer)
        {
            return _shopSessions.Find(x => x.Customer == customer);
        }
    }

}
