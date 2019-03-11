using System;
using System.Collections.Generic;
using System.Text;
using PDASimulator.DataStructures;
using PDASimulator.DataStructures.GSS;
using PDASimulator.DPDA.Simulation;
using PDASimulator.SimulationCommon;

namespace PDASimulator.Tests.Utils
{
    class TestCacheProvider<TState, TData, TPosition, TContext, TGssNode> : 
        ICacheProvider<TState, TData, TPosition, TContext, TGssNode> 
    {
        private readonly Dictionary<(TPosition, TState, TData), TContext> myVisits;
        private readonly Dictionary<(TPosition, TState, TGssNode), TContext> myVisitsOnPop;

        public TestCacheProvider()
        {
            myVisits = new Dictionary<(TPosition, TState, TData), TContext>();
            myVisitsOnPop = new Dictionary<(TPosition, TState, TGssNode), TContext>();
        }

        public bool Visited(TPosition position, TState inState, TData withData, out TContext byContext)
        {
            return myVisits.TryGetValue((position, inState, withData), out byContext);
        }

        public void Visit(TPosition position, TState state, TData data, 
            TContext context)
        {
            myVisits.Add((position, state, data), context);
        }

        public bool VisitedOnPop(TPosition position, TState targetState, TGssNode targetTop, out TContext byContext)
        {
            return myVisitsOnPop.TryGetValue((position, targetState, targetTop), out byContext);
        }

        public void VisitOnPop(TPosition position, TState targetState, TGssNode targetTop, TContext byContext)
        {
            myVisitsOnPop.Add((position, targetState, targetTop), byContext);
        }
    }
}
