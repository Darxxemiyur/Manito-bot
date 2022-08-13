namespace  Name.Bayfaderix.Darxxemiyur.Node.Network
{
    public struct NextNetworkInstruction
    {
        public readonly Node NextStep;
        public readonly NextNetworkActions NextAction;
        public readonly object Payload;
        public NextNetworkInstruction(Node nextStep, NextNetworkActions nextAction, object payload = null)
        {
            NextStep = nextStep;
            NextAction = nextAction;
            Payload = payload;
        }
    }
    public enum NextNetworkActions
    {
        Continue,
        Stop,
    }
}
