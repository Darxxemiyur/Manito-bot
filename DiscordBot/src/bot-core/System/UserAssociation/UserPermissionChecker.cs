using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Manito.Discord;

using System.Threading.Tasks;

namespace Manito.System.UserAssociation
{
	public class UserPermissionChecker
	{
		private readonly MyDomain _domain;

		public UserPermissionChecker(MyDomain domain) => _domain = domain;

		public Task<bool> IsGod(DiscordUser user) => Task.FromResult(860897395109789706 == user.Id);

		public async Task<bool> DoesHaveAdminPermission(object location, DiscordUser user, object salt = default)
		{
			if (await IsGod(user))
				return true;

			var guild = await _domain.MyDiscordClient.ManitoGuild;

			var guser = await guild.GetMemberAsync(user.Id, true);

			return guser.Permissions.HasFlag(Permissions.Administrator);
		}
	}
}