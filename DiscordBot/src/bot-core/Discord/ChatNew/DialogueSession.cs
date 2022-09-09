using Manito.Discord.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.Discord.ChatNew
{
	public class DialogueSession<T>
	{
		/// <summary>
		/// Responder that is used to generalize responding.
		/// </summary>
		public SessionResponder Responder {
			get; private set;
		}
		/// <summary>
		/// Puller of events for this Session
		/// </summary>
		public InteractionPuller Puller {
			get; private set;
		}
		/// <summary>
		/// Session information.
		/// </summary>
		public SessionInformation Information {
			get; private set;
		}
		/// <summary>
		/// Tab this session belongs to
		/// </summary>
		public DialogueSessionTab<T> Tab {
			get; private set;
		}
		/// <summary>
		/// Session context
		/// </summary>
		public T Context {
			get; private set;
		}
		/// <summary>
		/// Used to inform subscribers about session status change.
		/// </summary>
		public event Func<DialogueSession<T>, Task> OnStatusChange;
		public event Func<DialogueSession<T>, string, Task> OnSessionEnd;
		public DialogueSession(DialogueSessionTab<T> tab, InteractiveInteraction start, T context)
		{
			Tab = tab;
			Context = context;
			Information = new(tab.Client);
			Puller = new InteractionPuller(Information);
			Responder = new SessionResponder(Information, start);
		}
	}
}
