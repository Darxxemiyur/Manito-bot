
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Manito.Discord.UserAssociaton
{
    public class UserAssociatonCommands
    {
        public UserAssociatonCommands()
        {

        }

        public List<DiscordApplicationCommand> GetCommands()
        {
            throw new NotImplementedException();
        }

        public Func<DiscordInteraction, Task> Search(DiscordInteraction command)
        {
            throw new NotImplementedException();
        }

        public async Task EnterMenu()
        {

        }
        public async Task LinkAccount()
        {

        }
    }
}