

using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Manito.Discord.Client;

namespace Manito.Discord.Tickets
{
    public class Ticket
    {
        private DiscordChannel _channel;
        private DiscordUser _operator;
        private DiscordMember _user;
        private MyDiscordClient _wrapp;
        private ulong _categoryId = 958095775324336199;
        private DiscordChannel _category;

        public Ticket(MyDiscordClient wrapp, DiscordMember user)
        {
            _user = user;
            _wrapp = wrapp;
        }
        public async Task InitiateChannel()
        {
            var gld = await _wrapp.ManitoGuild;
            _category = gld.GetChannel(_categoryId);
            
            var roles = gld.Roles.Select(x => x.Value)
                .Where(x => x.CheckPermission(Permissions.Administrator) != PermissionLevel.Allowed);

            var overrids = roles.Select(x => new DiscordOverwriteBuilder(x)
                .Deny(Permissions.AccessChannels));

            overrids = overrids.Append(new DiscordOverwriteBuilder(_user)
            .Allow(Permissions.AccessChannels));

            _channel = await gld.CreateChannelAsync($"тикет-{_user.Id}",
                ChannelType.Text, _category, overwrites: overrids);
        }
        public async Task AssignOperator(DiscordUser toperator)
        {
            _operator = toperator;

        }
    }
}