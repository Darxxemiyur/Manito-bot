using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Emzi0767.Utilities;
using DSharpPlus.Interactivity.EventHandling;
using System.Threading;

namespace Manito.Discord.Client
{
    public class EventStopper<TEvent> : Predictator<TEvent> where TEvent : DiscordEventArgs
    {
        private Func<TEvent, bool> _predictator;
        private SemaphoreSlim _locker;
        private bool _runIfHandled;
        public EventStopper(Func<TEvent, bool> predictator, bool runIfHandled = false)
        {
            _locker = new(0, 1);
            _runIfHandled = runIfHandled;
            _predictator = predictator;
        }
        public override Task<bool> IsFitting(TEvent args)
        {
            var res = _predictator(args);

            _hasRan |= res;

            return Task.FromResult(res);
        }

        private bool _hasRan;

        public override bool RunIfHandled => _runIfHandled;

        public override Task<bool> IsREOL() => Task.FromResult(_hasRan);
    }
}