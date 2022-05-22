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

    public class AppArgsTools
    {
        private DiscordInteraction _intArgs;
        private IEnumerable<string> _reqArgs;
        private IEnumerable<string> _optArgs;
        public AppArgsTools(DiscordInteraction intArgs,
         IEnumerable<string> reqArgs, IEnumerable<string> optArgs)
        {
            _intArgs = intArgs;
            _reqArgs = reqArgs;
            _optArgs = optArgs;
        }
        public AppArgsTools(DiscordInteraction intArgs,
         IEnumerable<string> reqArgs) : this(intArgs, reqArgs, Array.Empty<string>())
        {
        }
        public AppArgsTools(DiscordInteraction intArgs) : this(intArgs, Array.Empty<string>())
        {
        }
        public AppArgsTools(DiscordInteraction intArgs, IEnumerable<(bool, string)> args)
        : this(intArgs,
         args.Where(x => x.Item1).Select(x => x.Item2),
         args.Where(x => !x.Item1).Select(x => x.Item2))
        {
        }
        public string AddReqArg(string argName)
        {
            _reqArgs = _reqArgs.Contains(argName) ? _reqArgs : _reqArgs.Append(argName);
            return argName;
        }
        public string AddOptArg(string argName)
        {
            _optArgs = _optArgs.Contains(argName) ? _optArgs : _optArgs.Append(argName);
            return argName;
        }
        public string AddArg(bool required, string argName) => required ? AddReqArg(argName) : AddOptArg(argName);
        private bool Recur(DiscordInteractionDataOption option, string arg)
        {
            return option.Name == arg || Recur(option.Options, arg);
        }
        private bool Recur(IEnumerable<DiscordInteractionDataOption> options, string arg)
        {
            return options?.Any(x => Recur(x, arg)) ?? false;
        }
        private object GetArg(DiscordInteractionDataOption option, string arg)
        {
            return option.Name == arg ? option.Value : GetArg(option.Options, arg);
        }
        private object GetArg(IEnumerable<DiscordInteractionDataOption> options, string arg)
        {
            return options?.Select(x => GetArg(x, arg))?.FirstOrDefault(x => x != null);
        }
        public bool DoHaveReqArgs() => _reqArgs.All(x => Recur(_intArgs.Data.Options, x));
        public bool AnyOptArgs() => _optArgs.Any(x => Recur(_intArgs.Data.Options, x));
        private IEnumerable<(string, object)> GetArgPairs(IEnumerable<string> tgt) =>
         tgt.Select(x => (x, GetArg(_intArgs.Data.Options, x))).Where(x => x.Item2 != null);
        public IDictionary<string, object> GetOptional() => GetArgPairs(_optArgs)
         .ToDictionary(x => x.Item1, x => x.Item2);
        public IDictionary<string, object> GetReq() => GetArgPairs(_reqArgs)
         .ToDictionary(x => x.Item1, x => x.Item2);
    }
}
