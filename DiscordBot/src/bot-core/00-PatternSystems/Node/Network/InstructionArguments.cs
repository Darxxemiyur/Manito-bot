namespace Name.Bayfaderix.Darxxemiyur.Node.Network
{
	public class NetworkInstructionArgument
	{
		public object Payload {
			get;
		}

		public NetworkInstructionArgument(object payload) => Payload = payload;

		public NetworkInstructionArgument(NextNetworkInstruction payload) => Payload = payload.Payload;
	}
}