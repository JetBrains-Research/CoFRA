using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Types;

namespace Cofra.AbstractIL.Internal.Resolvers
{
    public class ClassPropertyReferenceResolver : ReferenceResolver
    {
        protected override Type ProcessingType()
        {
            return typeof(ClassPropertyReference);
        }

        protected override Entity InternalResolve<TNode>(
            GraphStructuredProgram<TNode> program,
            ResolvedMethod<TNode> method,
            Reference reference)
        {
            var property = (ClassPropertyReference) reference;
            return Resolve(program, method, new ClassFieldReference(property.Owner, property.Name));
        }
    }
}
