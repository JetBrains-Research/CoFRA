namespace PDASimulator.SimulationCommon
{
    public class Head<TState, TPosition, TContext, TGssNode> 
        where TContext : class
    {
        public readonly TState State;
        public readonly TGssNode StackTop;
        public readonly TPosition Position;
        public TContext CurrentContext { get; set; }

        public Head(TState state, TGssNode stackTop, TPosition position)
        {
            State = state;
            StackTop = stackTop;
            Position = position;
            CurrentContext = null;
        }
    }
}