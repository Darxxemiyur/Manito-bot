using System.Threading.Tasks;

namespace  Name.Bayfaderix.Darxxemiyur.Node.Network
{
    public delegate Task<NextNetworkInstruction> Node(NetworkInstructionArguments args);
    public delegate Task<bool> NodeResultHandler(NextNetworkInstruction args);
}