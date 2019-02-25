using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Resolvers;
using Cofra.AbstractIL.Internal.Statements;
using Cofra.AbstractIL.Internal.Types;
using Cofra.AbstractIL.Internal.Types.Primaries;
using Cofra.AbstractIL.Internal.Types.Secondaries;

namespace Cofra.AbstractIL.Internal.Transducers
{
    public sealed class AssignmentsResolvingTransducer<TNode> : AbstractTransducer<TNode>
    {
        protected override bool Step(
            GraphStructuredProgram<TNode> targetProgram, 
            ResolvedMethod<TNode> targetMethod, 
            TNode source, 
            Statement statement,
            TNode target,
            Func<TNode> nodeCreator)
        {
            if (statement is AssignmentStatement assignment)
            {
                var sourceEntity = ReferenceResolver.Resolve(targetProgram, targetMethod, assignment.Source);
                var targetEntity = ReferenceResolver.Resolve(targetProgram, targetMethod, assignment.Target);

                var targetSecondaryEntity = targetEntity as SecondaryEntity;
                Trace.Assert(targetSecondaryEntity != null);

                var newStatement =
                    new ResolvedAssignmentStatement(assignment.Location, sourceEntity, targetSecondaryEntity);

                targetProgram.AddEdge(new OperationEdge<TNode>(source, newStatement, target));

                return false;
            }

            return true;
        }
    }
}
