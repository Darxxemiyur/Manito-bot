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
	public class BuyingStepsForError : IDialogueNet
	{
		private ShopSession _session;

		public BuyingStepsForError(ShopSession session) => _session = session;

		public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;
		public NextNetworkInstruction GetStartingInstruction(object payload) => GetStartingInstruction();
		public NextNetworkInstruction GetStartingInstruction() => new(SelectQuantity, NextNetworkActions.Continue);

		private async Task<NextNetworkInstruction> SelectQuantity(NetworkInstructionArgument args)
		{
			throw new NotImplementedException();
		}
	}
}