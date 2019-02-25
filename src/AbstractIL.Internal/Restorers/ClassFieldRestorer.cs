using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Types;
using Cofra.AbstractIL.Internal.Types.Primaries;
using Cofra.AbstractIL.Internal.Types.Secondaries;

namespace Cofra.AbstractIL.Internal.Restorers
{
    public sealed class ClassFieldRestorer : EntitiesRestorer
    {
        protected override Type GetProcessingType()
        {
            return typeof(ResolvedClassField);
        }

        protected override Entity RestoreImplementation<TNode>(
            GraphStructuredProgram<TNode> program,
            ResolvedMethod<TNode> method,
            Entity entity)
        {
            var classField = (ResolvedClassField) entity;

            var clazz = program.FindClassById(classField.ClassId);
            return clazz.GetOrCreateField(classField.VariableId);
        }
    }
}
