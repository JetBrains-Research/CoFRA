using PDASimulator.DPDA.Simulation;

namespace PDASimulator.SimulationCommon
{
    public interface IContextProcessor<TState, TStackSymbol, TPosition, TTransition, TContext, TGssNode>
        where TContext : class
    {
        TContext HeadToContext(Head<TState, TPosition, TContext, TGssNode> head);

        bool InheritContextsOnPush(
            TContext parent, 
            TContext child, 
            TState nextState, 
            TStackSymbol pushed,
            TTransition sourceTransition,
            bool memoized);

        bool InheritContextsOnPop(
            TContext parent, 
            TContext child, 
            TState nextState, 
            TTransition sourceTransition,
            bool memoized);

        bool InheritContextsOnSkip(
            TContext parent, 
            TContext child, 
            TState nextState, 
            TTransition sourceTransition,
            bool memoized);

        void MergeContexts(TContext source, TContext target);
        void MergeGssData(TGssNode source, TGssNode target);
    }
}