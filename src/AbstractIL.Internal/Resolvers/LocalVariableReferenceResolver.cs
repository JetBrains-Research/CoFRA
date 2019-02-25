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
using Cofra.AbstractIL.Internal.Types.Secondaries;

namespace Cofra.AbstractIL.Internal.Resolvers
{
    internal class LocalVariableReferenceResolver : ReferenceResolver
    {
        protected override Type ProcessingType()
        {
            return typeof(LocalVariableReference);
        }

        protected override Entity InternalResolve<TNode>(
            GraphStructuredProgram<TNode> program, 
            ResolvedMethod<TNode> method, 
            Reference reference)
        {
            var real = (LocalVariableReference) reference;
            var exists = method.Variables.TryGetValue(real.Index, out var resolved);

            Trace.Assert(exists);
            if (!exists)
            {
                method.AddLocalVariable(new ResolvedLocalVariable(real.Index));
                resolved = method.Variables[real.Index];
            }

            if (resolved.DefaultClassType < 0 && real.DefaultType != null)
            {
                var defaultType = program.GetOrCreateClass(real.DefaultType);
                resolved.DefaultClassType = defaultType.Id.GlobalId;
            }

            return resolved;
        }
    }
}
