using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Types;
using Cofra.AbstractIL.Internal.Types.Primaries;
using Cofra.AbstractIL.Internal.Types.Secondaries;

namespace Cofra.AbstractIL.Internal.Restorers
{
    public sealed class LocalVariableRestorer : EntitiesRestorer
    {
        protected override Type GetProcessingType()
        {
            return typeof(ResolvedLocalVariable);
        }

        protected override Entity RestoreImplementation<TNode>(
            GraphStructuredProgram<TNode> program, 
            ResolvedMethod<TNode> method, 
            Entity entity)
        {
            var localVariable = (ResolvedLocalVariable) entity;

            var exists = method.Variables.TryGetValue(localVariable.LocalId, out var variable);

            //Trace.Assert(exists);
            if (!exists)
            {
                method.AddLocalVariable(localVariable);
                variable = localVariable;
            }

            return variable;
        }
    }
}
