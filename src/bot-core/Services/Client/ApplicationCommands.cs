using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;

namespace Manito.Discord.Client
{

    public class ApplicationCommands
    {
        private MyService _collection;
        public DiscordClient Client => _collection.MyDiscordClient.Client;
        public ApplicationCommands(MyService collection)
        {
            _collection = collection;
            Commands = new Dictionary<String, IEnumerable<DiscordApplicationCommand>>();
        }
        public async Task UpdateCommands()
        {
            var commands = Commands.SelectMany(x => x.Value);

            await Client.BulkOverwriteGlobalApplicationCommandsAsync(commands);
        }
        public readonly Dictionary<String, IEnumerable<DiscordApplicationCommand>> Commands;
    }

}
