using System;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Types;
using Cofra.AbstractIL.Internal.Types.Primaries;
using Cofra.AbstractIL.Internal.Types.Secondaries;

namespace Cofra.AbstractIL.Internal.Restorers
{
    public class ObjectMethodReferenceRestorer : EntitiesRestorer
    {
        protected override Type GetProcessingType()
        {
            return typeof(ResolvedObjectMethodReference<int>);
        }

        protected override Entity RestoreImplementation<TNode>(
            GraphStructuredProgram<TNode> program,
            ResolvedMethod<TNode> method,
            Entity entity)
        {
            var objectMethodReference = (ResolvedObjectMethodReference<TNode>) entity;

            var restoredOwner = (SecondaryEntity) Restore(program, method, objectMethodReference.OwningObject);
            var restored = new ResolvedObjectMethodReference<TNode>(restoredOwner, objectMethodReference.MethodId)
            {
                FindClassMethod = program.FindMethodById
            };

            method.StoreAdditionalVariable(restored);

            return restored;
        }
    }
}
