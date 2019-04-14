using System;
using System.Collections.Generic;
using System.Linq;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Statements;
using Cofra.AbstractIL.Internal.Types.Markers;
using Cofra.AbstractIL.Internal.Types.Primaries;
using Cofra.AbstractIL.Internal.Types.Secondaries;
using Cofra.Core.Analyzes.PDVM;
using PDASimulator.DataStructures.GSS;
using PDASimulator.Payloads;
using PDASimulator.PDVM;
using PDASimulator.Utils;

namespace Cofra.Core.Analyzes.SourceFilterSink
{
    using Node = Int32;
    using State = SecondaryEntity;
    using Transition = OperationEdge<Int32>;
    using Context = PdaExtractingContext<GssNode<StackSymbol, EmptyGssData>, OperationEdge<Int32>>;
    using Result = IEnumerable<IEnumerable<Statement>>;

    public sealed class SourceFilterSink : 
        PdvmBasedAnalysis<State, StackSymbol, Context, EmptyGssData, Result>
    {
        private readonly GraphStructuredProgram<Node> myProgram;

        private static readonly Func<
            GraphStructuredProgram<int>, 
            Func<PDVM<State, StackSymbol, Node, Transition, Context, EmptyGssData>>> getPdvmProvider =
            program => () => new SourceFilterSinkPDVM(program);

        public SourceFilterSink(GraphStructuredProgram<int> program)
               : base(program, new SourceFilterSinkContextProcessor(), getPdvmProvider(program))
        {
            myProgram = program;
        }

        public override Result Analyze(IEnumerable<ResolvedMethod<int>> starts)
        {
            var initialState = new SourceFilterSinkPDVM.InitialDummyState();
            var initialStackData = new StackSymbol(new OperationEdge<Node>(-1, new NopStatement(), -1), null);
            var initialContexts = new List<Context>();
            var finalContexts = new List<Context>();

            foreach (var start in starts)
            {
                var rootContext = InternalSimulation.Load(start.EntryPoint, initialState, initialStackData);
                initialContexts.Add(rootContext);
            }

            var pdvm = (SourceFilterSinkPDVM) InternalSimulation.Pdvm;
            pdvm.OnFinishEvent += (head, lastTransition) => finalContexts.Add(head.CurrentContext);

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

            var formattedResults = results.Select(
                trace => trace
                    .SkipWhile(statement => statement is ResolvedInvocationStatement<Node>)
                    .Select<Statement, Statement>(statement =>
                    {
                        if (statement is ResolvedInvocationStatement<Node> invocation)
                            return new InfoStatement(invocation.Location, 
                                myProgram.Methods.GetKey(invocation.TargetMethodId.GlobalId));

                        if (statement is ResolvedAssignmentStatement assignment)
                            return new AssignmentStatement(assignment.Location, null, null);

                        if (statement is InternalReturnStatement)
                            return new ReturnStatement(statement.Location, null);

                        return null;
                    }).Where(statement => statement != null));

            return formattedResults;
        }
    }
}
