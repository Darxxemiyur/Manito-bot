using Manito.Discord.Client;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manito.Discord.ChatNew
{
	/// <summary>
	/// Dialogue Session tab Controls creation of new sessions and keeps the created ones.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class DialogueTabSessionTab<T>
	{
		public MyDiscordClient Client {
			get; private set;
		}

		// Keeps list of created sessions.
		private List<DialogueTabSession<T>> _sessions;

		public IReadOnlyList<DialogueTabSession<T>> Sessions => _sessions;

		// Used to sync creation and deletion of sessions
		private SemaphoreSlim _sync;

		public DialogueTabSessionTab(MyDiscordClient client)
		{
			_sync = new(1, 1);
			_sessions = new();
			Client = client;
		}

		public async Task<DialogueTabSession<T>> CreateSync(InteractiveInteraction interactive, T context)
		{
			await _sync.WaitAsync();

			var session = new DialogueTabSession<T>(this, interactive, context);
			session.OnRemove += RemoveSession;
			_sessions.Add(session);

			_sync.Release();

			return session;
		}

		public async Task<bool> RemoveSession(IDialogueSession session)
		{
			await _sync.WaitAsync();
			session.OnRemove -= RemoveSession;
			var res = _sessions.Remove(session as DialogueTabSession<T>);
			_sync.Release();

			return res;
		}
	}
}