using System.Collections.Generic;

namespace PDASimulator.SimulationCommon
{
    public interface ITransitionProvider<TPosition, TTransition>
    {
        IEnumerable<TTransition> Transitions(TPosition position);
        TPosition Target(TTransition transition);
    }
}