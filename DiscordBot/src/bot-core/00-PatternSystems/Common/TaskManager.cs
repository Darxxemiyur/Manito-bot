using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.EventArgs;


namespace Name.Bayfaderix.Darxxemiyur.Common
{
	/// <summary>
	/// Should manage tasks of TaskManagables the exact way they defined it.
	/// </summary>
	public class TaskManager
	{
		private IEnumerable<ITaskManagable> _taskManagables;
	}

}