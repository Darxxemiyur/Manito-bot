using Manito.Discord.Client;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Name.Bayfaderix.Darxxemiyur.Node.Linkable
{
	public interface INodeSystem : IModule
	{
		IEnumerable<INode> Network {
			get;
		}

		Task LinkSystem();
	}

	public abstract class BaseNodeSystem : IModule, INodeSystem
	{
		protected BaseNodeSystem()
		{
			_network = new();
		}

		public IEnumerable<INode> Network => _network;
		protected readonly List<INode> _network;

		public abstract Task LinkSystem();

		public Task RunModule() => Task.WhenAll(Network.Select(x => x.Run()));
	}
}