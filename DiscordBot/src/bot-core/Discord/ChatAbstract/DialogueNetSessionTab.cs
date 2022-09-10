using Manito.Discord.Chat.DialogueNet;
using Manito.Discord.ChatNew;
using Manito.Discord.Client;

using Microsoft.EntityFrameworkCore;

using Name.Bayfaderix.Darxxemiyur.Node.Network;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.Discord.ChatAbstract
{
	public class DialogueNetSessionTab<T>
	{
		private DialogueSessionTab<T> _sessionTab;
		private MyDomain _domain;

		public DialogueNetSessionTab(MyDomain domain)
		{
			_sessionTab = new(domain.MyDiscordClient);
			_domain = domain;
		}

		public async Task CreateSession(InteractiveInteraction interactive, T context,
			Func<DialogueSession<T>, Task<IDialogueNet>> builder)
		{
			var session = await _sessionTab.CreateSync(interactive, context);

			await _domain.ExecutionThread.AddNew(async () => {
				try
				{
					await NetworkCommon.RunNetwork(await builder(session));
				}
				catch (Exception e)
				{
					await session.Responder.SendMessage(new UniversalMessageBuilder()
						.SetContent("Сессия завершена из-за ошибки!"));
					await session.EndSession();

					throw new AggregateException(e);
				}
			});
		}
	}
}
