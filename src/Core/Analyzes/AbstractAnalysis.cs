using System;
using System.Collections.Generic;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Internal;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Types;

namespace Cofra.Core.Analyzes
{
    using Node = Int32;

    public abstract class AbstractAnalysis<TResult>
    {
        protected readonly GraphStructuredProgram<Node> Program;

        protected AbstractAnalysis(GraphStructuredProgram<Node> program)
        {
            Program = program;
        }

        public abstract TResult Analyze(IEnumerable<ResolvedMethod<Node>> starts);
    }
}
