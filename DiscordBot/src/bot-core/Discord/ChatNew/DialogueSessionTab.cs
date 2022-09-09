using Manito.Discord.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Manito.Discord.ChatNew
{
	/// <summary>
	/// Dialogue Session tab
	/// Controls creation of new sessions and keeps the created ones.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class DialogueSessionTab<T>
	{
		public MyDiscordClient Client {
			get; private set;
		}
		// Keeps list of created sessions.
		private List<DialogueSession<T>> _sessions;
		private IReadOnlyList<DialogueSession<T>> Sessions => _sessions;
		// Used to sync creation and deletion of sessions
		private SemaphoreSlim _sync;
		public SemaphoreSlim Sync => _sync;
		public DialogueSessionTab(MyDiscordClient client)
		{
			_sync = new(1, 1);
			_sessions = new();
			Client = client;
		}
		public async Task<DialogueSession<T>> Create(InteractiveInteraction interactive, T context)
		{
			await _sync.WaitAsync();

			var session = new DialogueSession<T>(this, interactive, context);
			_sessions.Add(session);

			_sync.Release();

			return session;
		}
		public async Task<bool> RemoveSession(DialogueSession<T> session)
		{
			await _sync.WaitAsync();
			var res = _sessions.Remove(session);
			_sync.Release();

			return res;
		}
	}
}
