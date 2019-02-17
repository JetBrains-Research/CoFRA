using PDASimulator.DataStructures.GSS;

namespace PDASimulator.DataStructures
{
    // TODO: Add extension through inheritance
    public class Context<TGssNode>
    {
        public readonly TGssNode StackTop;

        public Context(TGssNode stackTop)
        {
            StackTop = stackTop;
        }
    }
}