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
    public abstract class StatementRestorer
    {
        private static readonly Dictionary<InternalStatementType, StatementRestorer> RegisteredRestorers =
            new Dictionary<InternalStatementType, StatementRestorer>();

        private static void AddRestorer<TRestorer>()
            where TRestorer : StatementRestorer, new()
        {
            var instance = new TRestorer();
            RegisteredRestorers.Add(instance.ProcessingType(), instance);
        }

        static StatementRestorer()
        {
            AddRestorer<AssignmentsRestorer>();
            AddRestorer<InvocationsRestorer>();
        }

        protected abstract InternalStatementType ProcessingType();

        protected abstract InternalStatement RestoreImplementation<TNode>(
            GraphStructuredProgram<TNode> program,
            ResolvedMethod<TNode> method,
            InternalStatement statement);

        public static InternalStatement Restore<TNode>(
            GraphStructuredProgram<TNode> program,
            ResolvedMethod<TNode> method,
            InternalStatement statement)
        {
            var exists = RegisteredRestorers.TryGetValue(statement.InternalType, out var restorer);

            if (exists)
            {
                return restorer.RestoreImplementation(program, method, statement);
            }

            return statement;
        }
    }
}
