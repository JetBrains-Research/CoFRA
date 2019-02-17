using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Statements;
using Cofra.AbstractIL.Internal.Types;

namespace Cofra.AbstractIL.Internal.Restorers
{
    public abstract class EntitiesRestorer
    {
        private static readonly Dictionary<Type, EntitiesRestorer> RegisteredRestorers =
            new Dictionary<Type, EntitiesRestorer>();

        private static void AddRestorer<TRestorer>()
            where TRestorer : EntitiesRestorer, new()
        {
            var restorer = new TRestorer();
            var type = restorer.GetProcessingType();
            RegisteredRestorers[type] = restorer;
        }

        static EntitiesRestorer()
        {
            AddRestorer<ClassFieldRestorer>();
            AddRestorer<ClassMethodReferenceRestorer>();
            AddRestorer<LocalFunctionReferenceRestorer>();
            AddRestorer<LocalVariableRestorer>();
            AddRestorer<ObjectFieldRestorer>();
            AddRestorer<ObjectMethodReferenceRestorer>();
        }

        protected abstract Type GetProcessingType();

        protected abstract Entity RestoreImplementation<TNode>(
            GraphStructuredProgram<TNode> program,
            ResolvedMethod<TNode> method,
            Entity entity);

        public static Entity Restore<TNode>(
            GraphStructuredProgram<TNode> program,
            ResolvedMethod<TNode> method,
            Entity entity)
        {
            var exists = RegisteredRestorers.TryGetValue(entity.GetType(), out var restorer);

            if (exists)
            {
                return restorer.RestoreImplementation(program, method, entity);
            }

            return entity;
        }
    }
}
