using System;
using System.Collections.Generic;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Types;

namespace Cofra.AbstractIL.Internal.Resolvers
{
    public abstract class ReferenceResolver
    {
        private static readonly Dictionary<Type, ReferenceResolver> Resolvers =
            new Dictionary<Type, ReferenceResolver>();

        private static void AddResolver<TResolver>()
            where TResolver : ReferenceResolver, new()
        {
            var instance = new TResolver();
            Resolvers.Add(instance.ProcessingType(), instance);
        }

        static ReferenceResolver()
        {
            AddResolver<ClassReferenceResolver>();
            AddResolver<LocalVariableReferenceResolver>();
            //AddResolver<LocalFunctionReferenceResolver>();
            AddResolver<ClassFieldReferenceResolver>();
            AddResolver<ClassPropertyReferenceResolver>();
            AddResolver<ClassMethodReferenceResolver>();
            AddResolver<LocalFunctionReferenceResolver>();
        }

        protected abstract Type ProcessingType();

        public static Entity Resolve<TNode>(
            GraphStructuredProgram<TNode> program,
            ResolvedMethod<TNode> method,
            Reference reference)
        {
            if (reference == null)
            {
                Console.Error.WriteLine("Null reference");
                return null;
            }

            var type = reference.GetType();

            if (!Resolvers.ContainsKey(type))
            {
                Console.Error.WriteLine($"Cannot find resolver for reference type {type}");
            }

            var resolved = Resolvers[type].InternalResolve(program, method, reference);

            if (resolved == null)
            {
                Console.Error.WriteLine($"Reference of type {type} has been resolved as null");
            }

            return resolved;
        }

        protected abstract Entity InternalResolve<TNode>(
            GraphStructuredProgram<TNode> program,
            ResolvedMethod<TNode> method,
            Reference reference);
    }
}
