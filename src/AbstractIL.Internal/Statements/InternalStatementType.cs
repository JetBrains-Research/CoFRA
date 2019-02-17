using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofra.AbstractIL.Internal.Statements
{
    public enum InternalStatementType
    {
        ResolvedInvocation,
        ResolvedAssignment,
        Nop,
        Return
    }
}
