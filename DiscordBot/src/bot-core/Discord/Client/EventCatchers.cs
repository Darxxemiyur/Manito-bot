using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Common.Utilities;
using DisCatSharp.Interactivity.EventHandling;

namespace Manito.Discord.Client
{
    public class SingleEventCatcher<TEvent> : Predictator<TEvent> where TEvent : DiscordEventArgs
    {
        private Func<TEvent, bool> _predictator;
        private bool _runIfHandled;
        public SingleEventCatcher(Func<TEvent, bool> predictator, bool runIfHandled = false)
        {
            _runIfHandled = runIfHandled;
            _predictator = predictator;
        }
        public override Task<bool> IsFitting(DiscordClient client, TEvent args)
        {
            var res = _predictator(args);

            _hasRan = _hasRan || res;

            return Task.FromResult(res);
        }

        private bool _hasRan;

        public override bool RunIfHandled => _runIfHandled;

        public override Task<bool> IsREOL() => Task.FromResult(_hasRan);
    }

}