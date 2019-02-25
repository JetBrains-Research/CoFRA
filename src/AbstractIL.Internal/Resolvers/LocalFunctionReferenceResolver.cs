using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Types;
using Cofra.AbstractIL.Internal.Types.Primaries;

namespace Cofra.AbstractIL.Internal.Resolvers
{
    public sealed class LocalFunctionReferenceResolver : ReferenceResolver
    {
        protected override Type ProcessingType()
        {
            return typeof(LocalFunctionReference);
        }

        protected override Entity InternalResolve<TNode>(
            GraphStructuredProgram<TNode> program,
            ResolvedMethod<TNode> sourceMethod,
            Reference reference)
        {
            var localFunctionReference = (LocalFunctionReference) reference;

            var methodId = program.GetOrCreateMethodId(localFunctionReference.MethodId.Value);
            var method = sourceMethod.Methods[methodId];

            var resolvedReference = new ResolvedLocalFunctionReference<TNode>(methodId) {Method = method};

            return resolvedReference;
        }
    }
}
