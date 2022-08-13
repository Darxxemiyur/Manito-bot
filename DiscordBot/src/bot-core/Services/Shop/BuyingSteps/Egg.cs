using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

using Manito.Discord.Chat.DialogueNet;
using Name.Bayfaderix.Darxxemiyur.Node.Network;

namespace Manito.Discord.Shop
{
    public class BuyingStepsForEgg : IDialogueNet
    {
        private ShopItem _food;
        private ShopSession _session;
        private int _quantity;

        public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;
        public NextNetworkInstruction GetStartingInstruction(object payload) => GetStartingInstruction();

        public NextNetworkInstruction GetStartingInstruction() => new(ReturnBack, NextNetworkActions.Continue);
        public BuyingStepsForEgg(ShopSession session, ShopItem egg)
        {
            _session = session;
            _food = egg;
        }
        private async Task<NextNetworkInstruction> ReturnBack(NetworkInstructionArguments args)
        {
            var msg = _session.GetResponse(_session.BaseContent().WithDescription($"Вi ни можитi купить яйца!"));
            var accept = new DiscordButtonComponent(ButtonStyle.Danger, "Accept", "Ок.");

            msg.AddComponents(accept);
            await _session.Respond(msg);

            var aw = await _session.GetInteraction(x => x.Interaction.Data.CustomId == accept.CustomId);

            return new(null, NextNetworkActions.Stop);
        }
    }
}