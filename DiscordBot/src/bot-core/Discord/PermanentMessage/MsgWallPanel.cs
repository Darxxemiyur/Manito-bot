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
using Manito.Discord.ChatNew;

namespace Manito.Discord.PermanentMessage
{
	/// <summary>
	/// MessageWall Service Menu dialogue
	/// </summary>
	public class MsgWallPanel : IDialogueNet
	{
		private DialogueTabSession<MsgContext> _session;
		public MsgWallPanel(DialogueTabSession<MsgContext> session)
		{
			_session = session;
		}

		public NodeResultHandler StepResultHandler => Common.DefaultNodeResultHandler;

		public NextNetworkInstruction GetStartingInstruction() => new(SelectWhatToDo);
		public NextNetworkInstruction GetStartingInstruction(object payload) => GetStartingInstruction();
		private async Task<NextNetworkInstruction> SelectWhatToDo(NetworkInstructionArgument arg)
		{
			var wallLine = new DiscordButtonComponent(ButtonStyle.Primary, "wallLine", "Строки-сироты");
			var wall = new DiscordButtonComponent(ButtonStyle.Primary, "wall", "Стены");
			var imported = new DiscordButtonComponent(ButtonStyle.Primary, "import", "Импортированые соо-ния");
			var wallTranslator = new DiscordButtonComponent(ButtonStyle.Primary, "wallTranslator", "Трансляторы");

			var exitBtn = new DiscordButtonComponent(ButtonStyle.Danger, "exit", "Выйти");

			await _session.Responder.SendMessage(new DiscordInteractionResponseBuilder()
				.WithContent("Выберите что хотите изменять")
				.AddComponents(wallLine, wall, wallTranslator, imported)
				.AddComponents(exitBtn));
			var response = await _session.Puller.GetComponentInteraction();

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
				var next = new MsgWallPanelWallTranslator(_session);
				await NetworkCommon.RunNetwork(next);
			}

			if (response.CompareButton(imported))
			{
				var next = new MsgWallPanelWallLineImport(_session);
				await NetworkCommon.RunNetwork(next);
			}

			if (response.CompareButton(exitBtn))
				return new();

			return new(SelectWhatToDo);
		}
	}
}