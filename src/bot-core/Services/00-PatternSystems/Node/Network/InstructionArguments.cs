namespace Name.Bayfaderix.Darxxemiyur.Node.Network
{
    public class NetworkInstructionArguments
    {
        public object Payload { get; }
        public NetworkInstructionArguments(object payload) => Payload = payload;
        public NetworkInstructionArguments(NextNetworkInstruction payload) => Payload = payload.Payload;
    }
}