using System;
using System.Threading.Tasks;
using System.Linq;

using DSharpPlus;
using DSharpPlus.Entities;

using Manito.Discord.Client;
using Name.Bayfaderix.Darxxemiyur.Common;
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
        private bool _uirtt;
        public DiscordUser User => _user;
        public MyDiscordClient Client => _client;
        public DiscordInteraction Args => IArgs.Interaction;
        public InteractiveInteraction IArgs => _iargs;
        public ulong? GdId => _gdId;
        public ulong? ChId => _chId;
        public ulong? MsId => _msId;
        public bool IsEphemeral { get; }
        /// <summary>
        /// Create Dialogue network Session from Interactive args, bot client and sessioned user.
        /// </summary>
        /// <param name="iargs"></param>
        /// <param name="client"></param>
        /// <param name="user"></param>
        /// <param name="isEphemeral"></param>
        public DialogueNetSession(InteractiveInteraction iargs, MyDiscordClient client,
         DiscordUser user, bool isEphemeral = false)
        {
            _iargs = iargs;
            _client = client;
            _user = user;
            _gdId = null;
            _chId = null;
            _msId = null;
            IsEphemeral = isEphemeral;
        }
        private InteractionResponseType IRT => !_irtt && (_irtt = true)
            ? InteractionResponseType.ChannelMessageWithSource
            : InteractionResponseType.UpdateMessage;
        private InteractionResponseType UIRT => !_uirtt && (_uirtt = true)
            ? InteractionResponseType.DeferredChannelMessageWithSource
            : InteractionResponseType.DeferredMessageUpdate;

        public Task RespondLater() => Args.CreateResponseAsync(UIRT);
        public async Task Respond(DiscordInteractionResponseBuilder bld = default)
        {
            await Args.CreateResponseAsync(IRT, bld?.AsEphemeral(IsEphemeral));
            if (!IsEphemeral && _chId == null)
                _chId = (await Args.GetOriginalResponseAsync()).ChannelId;
            if (!IsEphemeral && _msId == null)
                _msId = (await Args.GetOriginalResponseAsync()).Id;
        }
        public virtual async Task QuitSession()
        {
            if (_chId != null && _msId != null && !IsEphemeral)
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
                 && (IsEphemeral || x.Message.Id == _msId) && checker(new(x.Interaction)));

            return _iargs = new(theEvent.Interaction);
        }
        public Task<InteractiveInteraction> GetInteraction() => GetInteraction(_ => true);
    }
}