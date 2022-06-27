using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    public class InteractiveInteraction
    {
        private readonly DiscordInteraction _interaction;
        public DiscordInteraction Interaction => _interaction;
        private DiscordInteractionData Data => _interaction.Data;
        public InteractiveInteraction(DiscordInteraction interaction)
        {
            _interaction = interaction;
        }
        public bool CompareButton(string name) => Data.CustomId == name;
        public bool CompareButton(DiscordButtonComponent btn) => CompareButton(btn.CustomId);
        public bool IsSelected(string option) => Data.Values.Any(x => x == option);
        public string[] GetSelected() => Data.Values;
        public string GetFirstSelected() => Data.Values.FirstOrDefault();
        public IEnumerable<TOption> GetButtons<TOption>(IDictionary<string, TOption> options)
         where TOption : class => options.Select(x => new Tuple<string, TOption>(x.Key, x.Value))
         .Where(y => CompareButton(y?.Item1)).Select(x => x?.Item2);
        public TOption GetButton<TOption>(IDictionary<string, TOption> options)
         where TOption : class => GetButtons(options).FirstOrDefault();
        public IEnumerable<TOption> GetOptions<TOption>(IDictionary<string, TOption> options)
         where TOption : class => options.Select(x => new Tuple<string, TOption>(x.Key, x.Value))
         .Where(y => Data.Values.Any(x => x == y.Item1)).Select(x => x?.Item2);
        public TOption GetOption<TOption>(IDictionary<string, TOption> options)
         where TOption : class => GetOptions(options).FirstOrDefault();
        public bool AnyComponents(IEnumerable<string> ids) => ids.Any(x => Data.CustomId == x);
        public bool AnyComponents(IEnumerable<DiscordComponent> components) =>
         AnyComponents(components.Select(x => x.CustomId));
        public bool AnyComponents(IEnumerable<DiscordActionRowComponent> components) =>
         AnyComponents(components.SelectMany(x => x.Components));
        //public DiscordComponent IsAnyPressed() => _interaction
    }
}
