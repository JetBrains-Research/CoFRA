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

    public sealed class SourceFilterSink : 
        PathsExtractingAnalysis<State, StackSymbol>
    {
        private readonly GraphStructuredProgram<Node> myProgram;

        private static readonly Func<
            GraphStructuredProgram<int>, 
            Func<SourceFilterSinkPDVM>> getPdvmProvider =
            program => () => new SourceFilterSinkPDVM(program);

        public SourceFilterSink(GraphStructuredProgram<int> program)
               : base(program, getPdvmProvider(program))
        {
            myProgram = program;
        }

        protected override IEnumerable<Statement> FormatTrace(IEnumerable<Statement> path)
        {
            return path
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
                    }).Where(statement => statement != null);
        }

        protected override State InitialState { get; } 
            = new SourceFilterSinkPDVM.InitialDummyState();

        protected override StackSymbol InitialStackSymbol { get; } 
            = new StackSymbol(new OperationEdge<Node>(-1, new NopStatement(), -1), null);
    }
}
