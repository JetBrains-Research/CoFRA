namespace PDASimulator.SimulationCommon
{
    public interface ICacheProvider<TState, TData, TPosition, TContext, TGssNode> 
    {
        bool Visited(TPosition position, TState inState, TData withData, 
            out TContext byContext);
        void Visit(TPosition position, TState state, TData data, 
            TContext context);

        bool VisitedOnPop(TPosition position, TState targetState, 
            TGssNode targetTop, 
            out TContext byContext);
        void VisitOnPop(TPosition position, TState targetState, 
            TGssNode targetTop, 
            TContext byContext);
    }
}