using Manito.Discord.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manito.Discord.ChatNew
{
	public class DialogueTabSession<T> : DialogueSession<T>
	{
		/// <summary>
		/// Tab this session belongs to
		/// </summary>
		public DialogueTabSessionTab<T> Tab {
			get; private set;
		}
		/// <summary>
		/// Used to inform subscribers about session status change.
		/// </summary>
		public new event Func<DialogueTabSession<T>, string, Task> OnStatusChange;
		public new event Func<DialogueTabSession<T>, string, Task> OnSessionEnd;
		public new event Func<DialogueTabSession<T>, Task<bool>> OnRemove;
		public DialogueTabSession(DialogueTabSessionTab<T> tab, InteractiveInteraction start, T context)
			: base(tab.Client, start, context)
		{
			Tab = tab;
			base.OnStatusChange += StatusChange;
			base.OnSessionEnd += SessionEnd;
			base.OnRemove += Remove;
		}
		private async Task StatusChange(DialogueSession<T> x, string y)
		{
			if (OnStatusChange != null)
				await OnStatusChange(x as DialogueTabSession<T>, y);
		}
		private async Task SessionEnd(DialogueSession<T> x, string y)
		{
			if (OnStatusChange != null)
				await OnSessionEnd(x as DialogueTabSession<T>, y);
		}
		private async Task<bool> Remove(DialogueSession<T> x)
		{
			return OnStatusChange != null ? await OnRemove(x as DialogueTabSession<T>) : false;
		}
	}
}
