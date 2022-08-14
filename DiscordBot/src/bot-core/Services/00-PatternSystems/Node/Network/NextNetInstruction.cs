namespace Name.Bayfaderix.Darxxemiyur.Node.Network
{
    public struct NextNetworkInstruction
    {
        public readonly Node NextStep;
        public readonly NextNetworkActions NextAction;
        public readonly object Payload;
        public NextNetworkInstruction(Node nextStep,
         NextNetworkActions nextAction = NextNetworkActions.Continue,
         object payload = null)
        {
            NextStep = nextStep;
            NextAction = nextAction;
            Payload = payload;
        }
        public NextNetworkInstruction(NextNetworkActions nextAction = NextNetworkActions.Stop)
        : this(null, nextAction) { }
    }
    public enum NextNetworkActions
    {
        Continue,
        Stop,
    }
}
