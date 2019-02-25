using System.Linq;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Statements;
using Cofra.AbstractIL.Internal.Types;
using Cofra.AbstractIL.Internal.Types.Primaries;
using Cofra.AbstractIL.Internal.Types.Secondaries;

namespace Cofra.AbstractIL.Internal.Restorers
{
    public class InvocationsRestorer : StatementRestorer
    {
        protected override InternalStatementType ProcessingType()
        {
            return InternalStatementType.ResolvedInvocation;
        }

        protected override InternalStatement RestoreImplementation<TNode>(
            GraphStructuredProgram<TNode> program, 
            ResolvedMethod<TNode> owner,
            InternalStatement statement)
        {
            var invocation = (ResolvedInvocationStatement<TNode>) statement;

            var restoredEntity = EntitiesRestorer.Restore(program, owner, invocation.TargetEntity);

            var restoredPassedParameters =
                invocation.PassedParameters.ToDictionary(
                    pair => (SecondaryEntity) EntitiesRestorer.Restore(program, owner, pair.Key),
                    pair => pair.Value);

            var restoredReturnedValues =
                invocation.ReturnedValues.ToDictionary(
                    pair => pair.Key,
                    pair => (SecondaryEntity) EntitiesRestorer.Restore(program, owner, pair.Value));

            return new ResolvedInvocationStatement<TNode>(
                invocation,
                restoredEntity,
                invocation.TargetMethodId,
                restoredPassedParameters,
                restoredReturnedValues);
        }
    }
}
