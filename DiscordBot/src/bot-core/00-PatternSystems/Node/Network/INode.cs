namespace Name.Bayfaderix.Darxxemiyur.Node.Network
{
	public interface INodeNetwork
	{
		NextNetworkInstruction GetStartingInstruction();

		NextNetworkInstruction GetStartingInstruction(object payload);

		NodeResultHandler StepResultHandler {
			get;
		}
	}
}