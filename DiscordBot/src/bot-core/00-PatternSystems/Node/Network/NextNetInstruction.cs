namespace Name.Bayfaderix.Darxxemiyur.Node.Network
{
	public struct NextNetworkInstruction
	{
		public readonly Node NextStep;
		public readonly NextNetworkActions NextAction;
		public readonly object Payload;

		public NextNetworkInstruction(NextNetworkInstruction parent)
		{
			NextStep = parent.NextStep;
			NextAction = parent.NextAction;
			Payload = parent.Payload;
		}

		public NextNetworkInstruction(Node nextStep, NextNetworkActions nextAction, object payload)
		{
			NextStep = nextStep;
			NextAction = nextAction;
			Payload = payload;
		}

		public NextNetworkInstruction(Node nextStep, object payload)
		{
			NextStep = nextStep;
			NextAction = NextNetworkActions.Continue;
			Payload = payload;
		}

		public NextNetworkInstruction(Node nextStep, NextNetworkActions nextAction)
		{
			NextStep = nextStep;
			NextAction = nextAction;
			Payload = null;
		}

		public NextNetworkInstruction(Node nextStep)
		{
			NextStep = nextStep;
			NextAction = nextStep != null ? NextNetworkActions.Continue : NextNetworkActions.Stop;
			Payload = null;
		}

		public NextNetworkInstruction(object payload)
		{
			NextStep = null;
			NextAction = NextNetworkActions.Stop;
			Payload = payload;
		}

		public NextNetworkInstruction()
		{
			NextStep = null;
			NextAction = NextNetworkActions.Stop;
			Payload = null;
		}

		public static implicit operator NextNetworkInstruction(Node input) => new(input);
	}

	public enum NextNetworkActions
	{
		Continue,
		Stop,
	}
}