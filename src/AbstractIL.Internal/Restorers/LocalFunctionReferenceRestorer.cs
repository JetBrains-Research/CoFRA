using System;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Types;

namespace Cofra.AbstractIL.Internal.Restorers
{
    public class LocalFunctionReferenceRestorer : EntitiesRestorer 
    {
        protected override Type GetProcessingType()
        {
            return typeof(ResolvedLocalFunctionReference<int>);
        }

        protected override Entity RestoreImplementation<TNode>(
            GraphStructuredProgram<TNode> program,
            ResolvedMethod<TNode> method,
            Entity entity)
        {
            var localFunctionReference = (ResolvedLocalFunctionReference<TNode>) entity;

            method.Methods.TryGetValue(localFunctionReference.MethodId, out var restored);
            localFunctionReference.Method = restored;
            return localFunctionReference;
        }
    }
}
