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
    public class ObjectFieldRestorer : EntitiesRestorer
    {
        protected override Type GetProcessingType()
        {
            return typeof(ResolvedObjectField);
        }

        protected override Entity RestoreImplementation<TNode>(
            GraphStructuredProgram<TNode> program,
            ResolvedMethod<TNode> method,
            Entity entity)
        {
            var objectField = (ResolvedObjectField) entity;

            var restoredOwner = (SecondaryEntity) Restore(program, method, objectField.OwningObject);
            var restored = new ResolvedObjectField(restoredOwner, objectField.FieldId)
            {
                FindClassField = program.FindClassFieldById
            };

            method.StoreAdditionalVariable(restored);

            return restored;
        }
    }
}
