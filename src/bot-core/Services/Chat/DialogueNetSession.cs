using System;
using System.Threading.Tasks;
using System.Linq;

using DSharpPlus;
using DSharpPlus.Entities;

using Manito.Discord.Client;
using System.Collections.Generic;
using Manito.Discord.Economy;
using DSharpPlus.EventArgs;
using Manito.Discord.Chat.DialogueNet;
using Manito.Discord.Inventory;

namespace Manito.Discord.Chat.DialogueNet
{
    public abstract class DialogueNetSession
    {

        protected DiscordUser _user;
        protected MyDiscordClient _client;
        protected InteractiveInteraction _iargs;
        protected ulong? _gdId;
        protected ulong? _chId;
        protected ulong? _msId;
        private bool _irtt;
        private bool _isEphemeral = false;
        public DiscordUser User => _user;
        public MyDiscordClient Client => _client;
        public DiscordInteraction Args => IArgs.Interaction;
        public InteractiveInteraction IArgs => _iargs;
        public ulong? GdId => _gdId;
        public ulong? ChId => _chId;
        public ulong? MsId => _msId;
        public bool IsEphemeral => _isEphemeral;
        public DialogueNetSession(InteractiveInteraction iargs, MyDiscordClient client,
         DiscordUser user, bool isEphemeral = false)
        {
            _iargs = iargs;
            _client = client;
            _user = user;
            _gdId = null;
            _chId = null;
            _msId = null;
            _isEphemeral = isEphemeral;
        }
        private InteractionResponseType GetIRT()
        {
            var irtt = _irtt;
            _irtt = true;
            return irtt ? InteractionResponseType.UpdateMessage
             : InteractionResponseType.ChannelMessageWithSource;
        }
        public async Task Respond(DiscordInteractionResponseBuilder bld = default)
        {
            await Args.CreateResponseAsync(GetIRT(), bld?.AsEphemeral(_isEphemeral));
            if (_chId == null)
                _chId = (await Args.GetOriginalResponseAsync()).ChannelId;
            if (_msId == null)
                _msId = (await Args.GetOriginalResponseAsync()).Id;
        }
        public virtual async Task QuitSession()
        {
            if (_chId != null && _msId != null && !_isEphemeral)
            {
                var chnl = await _client.Client.GetChannelAsync(_chId.Value);
                var msg = await chnl.GetMessageAsync(_msId.Value);
                await msg.DeleteAsync();
            }
            _irtt = false;
        }
        public Task<InteractiveInteraction> GetInteraction(
         IEnumerable<DiscordActionRowComponent> components) =>
         GetInteraction(x => x.AnyComponents(components));
        public async Task<InteractiveInteraction> GetInteraction(
         Func<InteractiveInteraction, bool> checker)
        {
            var theEvent = await _client.ActivityTools.WaitForComponentInteraction(x =>
                 x.User.Id == Args.User.Id && x.Message.ChannelId == _chId
                 && (_isEphemeral || x.Message.Id == _msId) && checker(new(x.Interaction)));

            return _iargs = new(theEvent.Interaction);
        }
        public Task<InteractiveInteraction> GetInteraction() => GetInteraction(_ => true);
    }
}