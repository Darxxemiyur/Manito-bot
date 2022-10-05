using System.Threading.Tasks;

namespace Name.Bayfaderix.Darxxemiyur.Node.Network
{
    public static class NetworkCommon
    {
        public static Task<object> RunNetwork(INodeNetwork net, object payload) => RunNetwork(net, net.StepResultHandler, payload);
        public static Task<object> RunNetwork(INodeNetwork net, NodeResultHandler handler, object payload)
        {
            return RunNetwork(net.GetStartingInstruction(payload), handler);
        }
        public static Task<object> RunNetwork(INodeNetwork net) => RunNetwork(net, net.StepResultHandler);
        public static Task<object> RunNetwork(INodeNetwork net, NodeResultHandler handler)
        {
            return RunNetwork(net.GetStartingInstruction(), handler);
        }
        private static async Task<object> RunNetwork(NextNetworkInstruction inst, NodeResultHandler handler)
        {
            while (await handler(inst))
                inst = await inst.NextStep(new(inst));
                
            return inst.Payload;
        }
    }
}