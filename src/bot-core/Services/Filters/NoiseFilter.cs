using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;

using Manito.Discord.Client;
using Manito.Discord.Economy;



namespace Manito.Discord.Filters
{

    public class NoiseFilter
    {
        public event Func<DiscordClient, MessageCreateEventArgs, Task> Filtered;
        public NoiseFilter(EventBuffer buffer)
        {
            buffer.Message.OnMessage += FilterMessage;
        }
        public async Task FilterMessage(DiscordClient client, MessageCreateEventArgs args)
        {
            if (args.Author.IsBot)
                return;



            if (Filtered != null)
                await Filtered(client, args);
        }
    }

}