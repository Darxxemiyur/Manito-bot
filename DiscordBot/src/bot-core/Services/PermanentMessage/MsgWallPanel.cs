using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using Name.Bayfaderix.Darxxemiyur.Common;
using System.Collections.Generic;
using Manito.Discord.Economy;
using DSharpPlus.EventArgs;
using Manito.Discord.Chat.DialogueNet;
using Manito.Discord.Inventory;
using Name.Bayfaderix.Darxxemiyur.Node.Network;
using System.Diagnostics;
using System;

namespace Manito.Discord.PermanentMessage
{
	/// <summary>
	/// MessageWall Service Menu dialogue
	/// </summary>
	public class MsgWallPanel : IDialogueNet
	{
		private MessageWallSession _session;
		public MsgWallPanel(MessageWallSession session)
		{
			_session = session;
		}

		public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;

		public NextNetworkInstruction GetStartingInstruction() => new(SelectWhatToDo);
		public NextNetworkInstruction GetStartingInstruction(object payload) => GetStartingInstruction();
		private async Task<NextNetworkInstruction> SelectWhatToDo(NetworkInstructionArgument arg)
		{
			var wallLine = new DiscordButtonComponent(ButtonStyle.Primary, "wallLine", "Единицу");
			var wall = new DiscordButtonComponent(ButtonStyle.Primary, "wall", "Набор единиц");
			var wallTranslator = new DiscordButtonComponent(ButtonStyle.Primary, "wallTranslator", "Переводчик");

			var exitBtn = new DiscordButtonComponent(ButtonStyle.Danger, "exit", "Выйти");

			var response = await _session.RespondAndWait(new DiscordInteractionResponseBuilder()
				.WithContent("Выбор изменения информации")
				.AddComponents(wallLine, wall, wallTranslator)
				.AddComponents(exitBtn));


			if (response.CompareButton(wallLine))
			{
				var next = new MsgWallPanelWallLine(_session);
				await NetworkCommon.RunNetwork(next);
			}

			if (response.CompareButton(wall))
			{
				var next = new MsgWallPanelWall(_session);
				await NetworkCommon.RunNetwork(next);
			}

			if (response.CompareButton(wallTranslator))
			{
				var next = new MsgWallPanelWallTranslator(_session, new(SelectWhatToDo));
				await NetworkCommon.RunNetwork(next);
			}

			if (response.CompareButton(exitBtn))
				return new();

			return new(SelectWhatToDo);
		}
	}
}