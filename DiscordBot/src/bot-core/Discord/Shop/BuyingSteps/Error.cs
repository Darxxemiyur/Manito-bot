using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;
using Microsoft.EntityFrameworkCore;

using Manito.Discord.Chat.DialogueNet;
using Name.Bayfaderix.Darxxemiyur.Node.Network;
using Manito.Discord.ChatNew;

namespace Manito.Discord.Shop
{
	public class BuyingStepsForError : IDialogueNet
	{
		private DialogueTabSession<ShopContext> _session;
		public BuyingStepsForError(DialogueTabSession<ShopContext> session) => _session = session;

		public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;
		public NextNetworkInstruction GetStartingInstruction(object payload) => GetStartingInstruction();
		public NextNetworkInstruction GetStartingInstruction() => new(SelectQuantity, NextNetworkActions.Continue);

		private async Task<NextNetworkInstruction> SelectQuantity(NetworkInstructionArgument args)
		{
			throw new NotImplementedException();
		}
	}
}