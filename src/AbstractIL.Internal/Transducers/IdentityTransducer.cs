using System;
using Cofra.AbstractIL.Common;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Types;
using Cofra.AbstractIL.Internal.Types.Primaries;

namespace Cofra.AbstractIL.Internal.Transducers
{
    public class IdentityTransducer<TNode> : AbstractTransducer<TNode>
    {
        protected override bool Step(
            GraphStructuredProgram<TNode> targetProgram, 
            ResolvedMethod<TNode> targetMethod,
            TNode source, Statement statement, TNode target,
            Func<TNode> nodeCreator)
        {
            targetProgram.AddOperation(source, new Operation<TNode>(statement, target));
            return true;
        }
    }
}
