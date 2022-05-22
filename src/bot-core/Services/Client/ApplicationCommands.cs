using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;

namespace Manito.Discord.Client
{

    public class ApplicationCommands
    {
        private MyDomain _collection;
        public DiscordClient Client => _collection.MyDiscordClient.Client;
        public ApplicationCommands(MyDomain collection)
        {
            _collection = collection;
            Commands = new Dictionary<String, IEnumerable<DiscordApplicationCommand>>();
        }
        public async Task UpdateCommands()
        {
            Client.Ready += DoUpdateCommands;
        }
        public void Add(string key, IEnumerable<DiscordApplicationCommand> value) =>
            Commands.Add(key, value);
        private async Task DoUpdateCommands(DiscordClient client, ReadyEventArgs args)
        {
            Client.Ready -= DoUpdateCommands;
            var commands = Commands.SelectMany(x => x.Value);
            args.Handled = true;
            await Client.BulkOverwriteGlobalApplicationCommandsAsync(commands);
        }
        public readonly Dictionary<String, IEnumerable<DiscordApplicationCommand>> Commands;
    }
}
