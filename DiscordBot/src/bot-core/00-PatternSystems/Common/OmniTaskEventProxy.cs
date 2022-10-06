using System.Collections.Generic;

namespace Name.Bayfaderix.Darxxemiyur.Common
{
	public class OmniTaskEventProxy<T>
	{
		private List<TaskEventProxy<T>> _channels;

		public OmniTaskEventProxy()
		{
			_channels = new();
		}
	}
}