using System.Collections.Generic;

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