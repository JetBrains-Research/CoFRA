using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Types.Primaries;
using PDASimulator.DataStructures.GSS;
using PDASimulator.Payloads;
using PDASimulator.PDVM;
using PDASimulator.SimulationCommon;
using PDASimulator.Utils;

namespace Cofra.Core.Analyzes.PDVM
{
    using Node = Int32;

    public abstract class PathsExtractingAnalysis<TState, TStackSymbol> : 
        PdvmBasedAnalysis<TState, TStackSymbol, 
            PdaExtractingContext<GssNode<TStackSymbol, EmptyGssData>, OperationEdge<Node>>,
            EmptyGssData,
            IEnumerable<IEnumerable<Statement>>>
    {
        protected PathsExtractingAnalysis(
            GraphStructuredProgram<int> program, 
            Func<ILProcessingPDVM<TState, TStackSymbol>> pdvmProvider) : 
            base(program, new PdaExtractingContextProcessor<TState, TStackSymbol, Node, OperationEdge<Node>>(), pdvmProvider)
        {
        }

        protected abstract TState InitialState { get; }
        protected abstract TStackSymbol InitialStackSymbol { get; }

        protected virtual IEnumerable<Statement> FormatTrace(IEnumerable<Statement> path)
        {
            return path;
        }

        public override IEnumerable<IEnumerable<Statement>> Analyze(IEnumerable<ResolvedMethod<Node>> starts)
        {
            var initialContexts = new List<PdaExtractingContext<GssNode<TStackSymbol, EmptyGssData>, OperationEdge<Node>>>();
            var finalContexts = new List<PdaExtractingContext<GssNode<TStackSymbol, EmptyGssData>, OperationEdge<Node>>>();

            foreach (var start in starts)
            {
                var context = InternalSimulation.Load(start.EntryPoint, InitialState, InitialStackSymbol);
                initialContexts.Add(context);
            }

            var pdvm = (ILProcessingPDVM<TState, TStackSymbol>) InternalSimulation.Pdvm;
            pdvm.OnFinishEvent += context => finalContexts.Add(context);

            InternalSimulation.Run();

            var finalContextSet = finalContexts.ToHashSet();
            var results = new List<IEnumerable<Statement>>();
            foreach (var initialContext in initialContexts)
            {
                PdaContextDecoder.ExtractWords(
                    initialContext,
                    context => finalContextSet.Contains(context),
                    0,
                    word =>
                    {
                        results.Add(word.Select(symbol => symbol?.Statement));
                        return true;
                    },
                    _ => { });
            }

            return results.Select(FormatTrace);
        }
    }
}
