using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Types;

namespace Cofra.AbstractIL.Internal.Resolvers
{
    public class ClassMethodReferenceResolver : ReferenceResolver
    {
        protected override Type ProcessingType()
        {
            return typeof(ClassMethodReference);
        }

        protected override Entity InternalResolve<TNode>(
            GraphStructuredProgram<TNode> program,
            ResolvedMethod<TNode> sourceMethod,
            Reference reference)
        {
            var classMethodReference = (ClassMethodReference) reference;

            var resolvedOwner = Resolve(program, sourceMethod, classMethodReference.Owner);

            if (resolvedOwner is ResolvedClassId owningClassId)
            {
                var owner = program.FindClassById(owningClassId);

                var method = program.GetOrCreateMethod(owner, classMethodReference.Name);
                var methodId = program.GetOrCreateMethodId(classMethodReference.Name);

                var resolvedReference = new ResolvedClassMethodReference<TNode>(owningClassId, methodId);
                resolvedReference.Method = method;

                return resolvedReference;
            }

            if (resolvedOwner is SecondaryEntity owningEntity)
            {
                var methodId = program.GetOrCreateMethodId(classMethodReference.Name);

                var resolvedReference = new ResolvedObjectMethodReference<TNode>(owningEntity, methodId);
                resolvedReference.FindClassMethod = program.FindMethodById;

                sourceMethod.StoreAdditionalVariable(resolvedReference);
                return resolvedReference;
            }

            return null;
        }
    }
}
