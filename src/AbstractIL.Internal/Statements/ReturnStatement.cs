using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.Types;

namespace Cofra.AbstractIL.Internal.Statements
{
    [DataContract]
    public class InternalReturnStatement : InternalStatement
    {
        public InternalReturnStatement() : base(null)
        {
        }

        public override InternalStatementType InternalType => InternalStatementType.Return;
    }
}
