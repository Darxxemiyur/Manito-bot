using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
