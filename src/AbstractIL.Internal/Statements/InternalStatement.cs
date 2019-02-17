using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Common.Types;

namespace Cofra.AbstractIL.Internal.Statements
{
    [DataContract]
    [KnownType(typeof(NopStatement))]
    [KnownType(typeof(InternalReturnStatement))]
    [KnownType(typeof(ResolvedAssignmentStatement))]
    [KnownType(typeof(ResolvedInvocationStatement<int>))]
    public abstract class InternalStatement : Statement
    {
        protected InternalStatement(Location location) : base(location)
        {
        }

        public override StatementType Type => StatementType.Internal;
        public abstract InternalStatementType InternalType { get; }
    }
}
