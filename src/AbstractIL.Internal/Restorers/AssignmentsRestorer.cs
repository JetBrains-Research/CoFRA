using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Statements;
using Cofra.AbstractIL.Internal.Types;
using Cofra.AbstractIL.Internal.Types.Primaries;
using Cofra.AbstractIL.Internal.Types.Secondaries;

namespace Cofra.AbstractIL.Internal.Restorers
{
    public sealed class AssignmentsRestorer : StatementRestorer
    {
        protected override InternalStatementType ProcessingType()
        {
            return InternalStatementType.ResolvedAssignment;
        }

        protected override InternalStatement RestoreImplementation<TNode>(
            GraphStructuredProgram<TNode> program, 
            ResolvedMethod<TNode> method,
            InternalStatement statement)
        {
            var assignment = (ResolvedAssignmentStatement) statement;

            var source = EntitiesRestorer.Restore(program, method, assignment.Source);
            Trace.Assert(assignment.Target != null);
            var target = assignment.Target == null ? null : EntitiesRestorer.Restore(program, method, assignment.Target);

            Trace.Assert(target is SecondaryEntity);

            return new ResolvedAssignmentStatement(
                assignment.Location, source, (SecondaryEntity) target, assignment.TargetReferencedByThis);
        }
    }
}
