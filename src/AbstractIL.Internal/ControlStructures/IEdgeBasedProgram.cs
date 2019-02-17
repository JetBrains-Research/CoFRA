using System.Collections.Generic;

namespace Cofra.AbstractIL.Internal.ControlStructures
{
    public interface IEdgeBasedProgram<TPosition>
    {
        IEnumerable<Operation<TPosition>> PossibleOperations(TPosition position);
        IEnumerable<TPosition> GetStarts();
    }
}
