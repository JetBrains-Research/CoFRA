using System;
using Cofra.AbstractIL.Internal;
using Cofra.AbstractIL.Internal.ControlStructures;
using PDASimulator.DataStructures;
using PDASimulator.DataStructures.GSS;
using PDASimulator.PDVM;
using PDASimulator.PDVM.Simulation;
using PDASimulator.SimulationCommon;

namespace Cofra.Core.Analyzes.PDVM
{
    using Node = Int32;

    public abstract class PdvmBasedAnalysis<TState, TStackSymbol, TContext, TGssData, TResult> 
        : AbstractAnalysis<TResult> 
        where TContext : Context<GssNode<TStackSymbol, TGssData>> 
        where TGssData : new()
    {
        protected readonly ICacheProvider<TState, TStackSymbol, Node,
            TContext, GssNode<TStackSymbol, TGssData>> InternalCacheProvider;

        protected readonly ITransitionProvider<Node, OperationEdge<Node>>
            InternalTransitionProvider;

        protected readonly PDVMSimulation<TState, TStackSymbol, Node, OperationEdge<Node>, 
            TContext, TGssData> InternalSimulation;

        protected PdvmBasedAnalysis(
            GraphStructuredProgram<int> program, 
            IContextProcessor<TState, TStackSymbol, Node, OperationEdge<Node>, 
                TContext, GssNode<TStackSymbol, TGssData>> contextProcessor,
            Func<PDVM<TState, TStackSymbol, Node, OperationEdge<Node>, TContext, TGssData>> pdvmProvider)
            : base(program)
        {
            InternalCacheProvider = new CacheProvider<TState, TStackSymbol, TContext, GssNode<TStackSymbol, TGssData>>();
            InternalTransitionProvider = new TransitionProvider(program);

            InternalSimulation = new PDVMSimulation<TState, TStackSymbol, int, OperationEdge<int>, TContext, TGssData>(
                contextProcessor, InternalCacheProvider, InternalTransitionProvider, pdvmProvider);
        }
    }
}
