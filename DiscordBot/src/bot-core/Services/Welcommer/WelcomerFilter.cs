﻿using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Manito.Discord;
using Manito.Discord.Client;

using Name.Bayfaderix.Darxxemiyur.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.Discord.Welcommer
{
	public class WelcomerFilter : IModule
	{
		public const ulong NimfaRole = 915918629172822036;
		public const string WelcomeMessage = "Добро пожаловать <@{0}> на наш {1} проект!";
		private MyDiscordClient _client;
		private TaskEventProxy<DiscordMember> _toAddQueue;
		public WelcomerFilter(MyDiscordClient client)
		{
			_client = client;
			_toAddQueue = new();
			client.Client.GuildMemberAdded += OnNewNymfJoin;
		}

		private async Task OnNewNymfJoin(DiscordClient sender, GuildMemberAddEventArgs e)
		{
			if (e.Guild.Id == (await _client.ManitoGuild).Id)
				await _toAddQueue.Handle(e.Member);

			e.Handled = true;
		}

		public async Task RunModule()
		{
			while (true)
			{
				try
				{
					var member = await _toAddQueue.GetData();
					await member.SendMessageAsync(string.Format(WelcomeMessage, member.Id,
						member.Guild.Name));
					await member.GrantRoleAsync(member.Guild.GetRole(NimfaRole));
				}
				catch (Exception)
				{

				}
			}
		}
	}
}