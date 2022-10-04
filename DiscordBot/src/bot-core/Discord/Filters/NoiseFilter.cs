using System;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.EventArgs;

using Manito.Discord.Client;
using Name.Bayfaderix.Darxxemiyur.Common;
using Manito.System.Economy; using Manito.Discord;



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