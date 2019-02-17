using System;
using System.Collections.Generic;
using System.Text;
using PDASimulator.DataStructures;
using PDASimulator.DataStructures.GSS;
using PDASimulator.SimulationCommon;

namespace PDASimulator.PDVM
{
    public abstract class PDVMState<TState, TStackSymbol, TPosition, TTransition, TContext, TGssData>
        where TGssData : new() 
        where TContext : Context<GssNode<TStackSymbol, TGssData>>
    {
        public abstract IContextProcessor<TState, TStackSymbol, TPosition, 
            TTransition, TContext, GssNode<TStackSymbol, TGssData>> ContextProcessor { get; }

        public abstract ICacheProvider<TState, TStackSymbol, TPosition, 
            TContext, GssNode<TStackSymbol, TGssData>> CacheProvider { get; }

        public abstract PopHistory<TState, TTransition, TPosition, TContext, GssNode<TStackSymbol, TGssData>> PopHistory { get; }

        public abstract TTransition CurrentTransition { get; }
        public abstract TPosition NextPosition { get; set; }

        public abstract Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>> CurrentHead { get; }

        public abstract void PushHead(Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>> head);
    }
}
