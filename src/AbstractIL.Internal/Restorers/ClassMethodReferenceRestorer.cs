using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Types;

namespace Cofra.AbstractIL.Internal.Restorers
{
    public class ClassMethodReferenceRestorer : EntitiesRestorer
    {
        protected override Type GetProcessingType()
        {
            return typeof(ResolvedClassMethodReference<int>);
        }

        protected override Entity RestoreImplementation<TNode>(
            GraphStructuredProgram<TNode> program, 
            ResolvedMethod<TNode> method, 
            Entity entity)
        {
            var classMethodReference = (ResolvedClassMethodReference<TNode>) entity;

            classMethodReference.Method =
                program.FindMethodById(classMethodReference.OwnerId, classMethodReference.MethodId);

            return classMethodReference;
        }
    }
}
