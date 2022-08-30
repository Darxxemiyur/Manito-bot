using DSharpPlus;
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
		public const ulong WayChannel = 1006205301206306847;
		public const ulong NewsChannel = 915691089082581032;
		public const ulong RestartsChannel = 915691265016877078;
		public const ulong DRulesChannel = 915690019476348948;
		public const ulong GRulesChannel = 915689991441621025;
		public const ulong PunishChannel = 915690112531198012;
		public const ulong LimitsChannel = 915693337783185429;
		public const ulong RolesChannel = 916297964073385984;
		public const ulong PrayChannel = 915691397061935204;

		public static string M1Ch => $"<#{WayChannel}>\n<#{NewsChannel}>\n<#{RestartsChannel}>";
		public static string M2Ch => $"<#{DRulesChannel}>\n<#{GRulesChannel}>\n<#{PunishChannel}>";
		public static string M3Ch => $"<#{LimitsChannel}>\n<#{RolesChannel}>\n<#{PrayChannel}>";
		public static string MCh => $"Так-же для вашего удобства ознакомьтесь с\n{M1Ch}\n{M2Ch}\n{M3Ch}";
		public static string WelcomeMessage => "Добро пожаловать <@{0}> на наш {1} проект!\n" + MCh;
		private MyDiscordClient _client;
		private TaskEventProxy<DiscordMember> _toAddQueue;
		public WelcomerFilter(MyDiscordClient client)
		{
			_client = client;
			_toAddQueue = new();
#if !DEBUG
			client.Client.GuildMemberAdded += OnNewNymfJoin;
#endif

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
					var guild = await _client.ManitoGuild;
					await member.SendMessageAsync(string.Format(WelcomeMessage, member.Id, guild.Name));
					await member.GrantRoleAsync(guild.GetRole(NimfaRole));
				}
				catch { }
			}
		}
	}
}