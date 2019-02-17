using System.Collections.Generic;
using Cofra.AbstractIL.Common.Statements;

namespace Cofra.AbstractIL.Internal.ControlStructures
{
    public interface INodeBasedProgram<TPosition>
    {
        Statement StatementAt(TPosition position);
        IEnumerable<TPosition> Transitions(TPosition position);
        IEnumerable<TPosition> GetStarts();
        bool IsFinal(TPosition position);
    }
}