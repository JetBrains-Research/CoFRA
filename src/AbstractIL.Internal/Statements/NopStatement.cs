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
    public sealed class NopStatement : InternalStatement
    {
        public NopStatement() : base(null)
        {
        }

        public override InternalStatementType InternalType => InternalStatementType.Nop;

        public override bool Equals(object obj)
        {
            return obj is NopStatement;
        }

        public override int GetHashCode()
        {
            var hashCode = -1670874114;
            return hashCode;
        }
    }
}
