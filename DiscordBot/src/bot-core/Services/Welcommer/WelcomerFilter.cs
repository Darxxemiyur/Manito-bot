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

using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;

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
		public const ulong HelpChannel = 915692365048578099;

		public static ulong ManitoGuild = 915355370673811486;
		public static ulong TrailerMSG = 1014143520744939541;
		public static ulong NoticeMSG = 1013114270331969606;
		public static string ChannelLink => $"https://discord.com/channels/{ManitoGuild}";
		public static string NewsChannelLink => $"{ChannelLink}/{NewsChannel}";
		public static string TrailerlLink => $"{NewsChannelLink}/{TrailerMSG}";
#if DEBUG
		public static string Er => "1007397905654620260";
#else
		public static string Er => "1007401635443638432";
#endif
		public static string E => $"<:{Er}:{Er}>";
		public static string NoticeLink => $"{NewsChannelLink}/{NoticeMSG}";
		public static string HelpChannelLink = $"{ChannelLink}/{ManitoGuild}/{HelpChannel}/1013056576719949895";
		public static string M1Ch => $"{E}<#{WayChannel}>\n{E}<#{NewsChannel}>\n{E}<#{RestartsChannel}>";
		public static string M2Ch => $"{E}<#{DRulesChannel}>\n{E}<#{GRulesChannel}>\n{E}<#{PunishChannel}>";
		public static string M3Ch => $"{E}<#{LimitsChannel}>\n{E}<#{RolesChannel}>\n{E}<#{PrayChannel}>";
		public static string MCh => $"Для вашего удобства рекомендуется ознакомиться с:\n{M1Ch}\n{M2Ch}\n{M3Ch}";
		public static string WelcomeMessage => "<@{0}>\nДобро пожаловать на наш {1} проект!\n" + MCh;
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

		public async Task<(DiscordGuild, DiscordMessageBuilder[])> GetMsg(ulong member)
		{
			var guild = await _client.ManitoGuild;

			var msg1 = new DiscordMessageBuilder();

			var n1 = new DiscordLinkButtonComponent(NoticeLink, "Новость касательно экономики");
			var n2 = new DiscordLinkButtonComponent(TrailerlLink, "Трейлер функционала бота");
			var n3 = new DiscordLinkButtonComponent(HelpChannelLink, "Обратиться за помощью в нашем канале!", false, new DiscordComponentEmoji("⛪"));

			var icon = guild.GetIconUrl(ImageFormat.Auto);

			msg1.WithEmbed(new DiscordEmbedBuilder()
				.WithDescription(string.Format(WelcomeMessage, member, guild.Name))
				.WithFooter("*Важно глянуть две кнопочки ниже!*")
				.WithThumbnail(icon)
				.WithColor(new DiscordColor("#a91dde")));

			msg1.AddComponents(n1, n2);

			var msg2 = new DiscordMessageBuilder();

			var m1 = "В случае возникновения вопросов __**не**__ стоит обращаться к Администрации в личные сообщения.";
			var m2 = $"Следует обратиться в <#{HelpChannel}> чтобы вам помог свободный администратор!";
			msg2.WithEmbed(new DiscordEmbedBuilder()
				.AddField("**ВНИМАНИЕ**", $"{m1}\n{m2}")
				.WithColor(DiscordColor.CornflowerBlue));

			msg2.AddComponents(n3);

			return (guild, new[] { msg1, msg2 });
		}

		public async Task RunModule()
		{
			while (true)
			{
				var member = await _toAddQueue.GetData();

				try
				{
					var (guild, msgs) = await GetMsg(member.Id);
					try
					{

						foreach (var msg in msgs)
							await member.SendMessageAsync(msg);
					}
					catch { }
					try
					{
						await member.GrantRoleAsync(guild.GetRole(NimfaRole));
					}
					catch { }
				}
				catch { }
			}
		}
	}
}