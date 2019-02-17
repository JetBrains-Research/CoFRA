using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Resolvers;
using Cofra.AbstractIL.Internal.Types;

namespace Cofra.AbstractIL.Internal.Resolvers
{
    public class ClassFieldReferenceResolver : ReferenceResolver
    {
        protected override Type ProcessingType()
        {
            return typeof(ClassFieldReference);
        }

        protected override Entity InternalResolve<TNode>(
            GraphStructuredProgram<TNode> program, 
            ResolvedMethod<TNode> method, 
            Reference reference)
        {
            var classFieldReference = (ClassFieldReference) reference;
            if (classFieldReference.Owner is ClassReference classReference)
            {
                var clazz = program.GetOrCreateClass(classReference.ClassId);
                var field = program.GetOrCreateClassField(clazz.Id, classFieldReference.Name);

                if (classFieldReference.DefaultClassType != null)
                {
                    field.DefaultClassType = program.GetOrCreateClass(classFieldReference.DefaultClassType).Id.GlobalId;
                }

                return field;
            }
            else if (classFieldReference.Owner is LocalVariableReference localVariableReference)
            {
                var localVariable = method.Variables[localVariableReference.Index];
                var fieldId = program.GetOrCreateFieldId(classFieldReference.Name);

                var resolvedObjectField = new ResolvedObjectField(localVariable, fieldId)
                {
                    FindClassField = program.FindClassFieldById
                };

                method.StoreAdditionalVariable(resolvedObjectField);
                return resolvedObjectField;
            }
            else if (classFieldReference.Owner is ClassFieldReference anotherReference)
            {
                var resolvedAnotherReference = (SecondaryEntity) Resolve(program, method, anotherReference);
                var fieldId = program.GetOrCreateFieldId(classFieldReference.Name);

                var resolvedObjectField = new ResolvedObjectField(resolvedAnotherReference, fieldId)
                {
                    FindClassField = program.FindClassFieldById
                };

                method.StoreAdditionalVariable(resolvedObjectField);
                return resolvedObjectField;
            }
            else if (classFieldReference.Owner is ClassPropertyReference propertyReference)
            {
                var resolvedAnotherReference = (SecondaryEntity) Resolve(program, method, propertyReference);
                var fieldId = program.GetOrCreateFieldId(classFieldReference.Name);

                var resolvedObjectField = new ResolvedObjectField(resolvedAnotherReference, fieldId)
                {
                    FindClassField = program.FindClassFieldById
                };

                method.StoreAdditionalVariable(resolvedObjectField);
                return resolvedObjectField;
            }

            Trace.Assert(false);
            return null;
        }
    }
}
