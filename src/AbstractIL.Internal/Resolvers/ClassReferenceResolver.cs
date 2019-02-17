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
    public class ClassReferenceResolver : ReferenceResolver
    {
        protected override Type ProcessingType()
        {
            return typeof(ClassReference);
        }

        protected override Entity InternalResolve<TNode>(
            GraphStructuredProgram<TNode> program,
            ResolvedMethod<TNode> method,
            Reference reference)
        {
            var real = (ClassReference) reference;

            return program.GetOrCreateClass(real.ClassId).Id;
        }
    }
}
