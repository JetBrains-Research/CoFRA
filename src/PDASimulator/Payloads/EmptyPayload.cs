using System;
using System.Collections.Generic;
using System.Text;
using PDASimulator.DataStructures;
using PDASimulator.DataStructures.GSS;
using PDASimulator.DPDA;
using PDASimulator.DPDA.Simulation;
using PDASimulator.SimulationCommon;

namespace PDASimulator.Payloads
{
    public struct EmptyGssData 
    {
    }

    public class EmptyContextProcessor<TState, TStackSymbol, TPosition, TTransition>
        : IContextProcessor<TState, TStackSymbol, TPosition, TTransition, Context<GssNode<TStackSymbol, EmptyGssData>>, GssNode<TStackSymbol, EmptyGssData>>
    {
        public Context<GssNode<TStackSymbol, EmptyGssData>> HeadToContext(Head<TState, TStackSymbol, Context<GssNode<TStackSymbol, EmptyGssData>>, GssNode<TStackSymbol, EmptyGssData>> head)
        {
            return new Context<GssNode<TStackSymbol, EmptyGssData>>(head.StackTop);
        }

        public Context<GssNode<TStackSymbol, EmptyGssData>> HeadToContext(Head<TState, TPosition, Context<GssNode<TStackSymbol, EmptyGssData>>, GssNode<TStackSymbol, EmptyGssData>> head)
        {
            return new Context<GssNode<TStackSymbol, EmptyGssData>>(head.StackTop);
        }

        public bool InheritContextsOnPush(Context<GssNode<TStackSymbol, EmptyGssData>> parent, Context<GssNode<TStackSymbol, EmptyGssData>> child, TState nextState, TStackSymbol pushed,
            TTransition sourceTransition, bool memoized)
        {
            return true;
        }

        public bool InheritContextsOnPop(Context<GssNode<TStackSymbol, EmptyGssData>> parent, Context<GssNode<TStackSymbol, EmptyGssData>> child, TState nextState, TTransition sourceTransition, bool memoized)
        {
            return true;
        }

        public bool InheritContextsOnSkip(Context<GssNode<TStackSymbol, EmptyGssData>> parent, Context<GssNode<TStackSymbol, EmptyGssData>> child, TState nextState, TTransition sourceTransition,
            bool memoized)
        {
            return true;
        }

        public void MergeContexts(Context<GssNode<TStackSymbol, EmptyGssData>> source, Context<GssNode<TStackSymbol, EmptyGssData>> target)
        {
        }

        public void MergeGssData(GssNode<TStackSymbol, EmptyGssData> source, GssNode<TStackSymbol, EmptyGssData> target)
        {
        }
    }
}
