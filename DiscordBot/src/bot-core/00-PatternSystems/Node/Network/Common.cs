using System.Threading;
using System.Threading.Tasks;

namespace Name.Bayfaderix.Darxxemiyur.Node.Network
{
	public static class NetworkCommon
	{
		public static Task<object> RunNetwork(INodeNetwork net, object payload, CancellationToken token = default) => RunNetwork(net, net.StepResultHandler, payload, token);

		public static Task<object> RunNetwork(INodeNetwork net, NodeResultHandler handler, object payload, CancellationToken token = default)
		{
			return RunNetwork(net.GetStartingInstruction(payload), handler, token);
		}

		public static Task<object> RunNetwork(INodeNetwork net, CancellationToken token = default) => RunNetwork(net, net.StepResultHandler, token);

		public static Task<object> RunNetwork(INodeNetwork net, NodeResultHandler handler, CancellationToken token = default)
		{
			return RunNetwork(net.GetStartingInstruction(), handler, token);
		}

		private static async Task<object> RunNetwork(NextNetworkInstruction inst, NodeResultHandler handler, CancellationToken token = default)
		{
			while (await handler(inst, token))
				inst = await inst.NextStep(new(inst));

			return inst.Payload;
		}
	}
}