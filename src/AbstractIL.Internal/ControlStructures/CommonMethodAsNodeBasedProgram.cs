using System.Collections.Generic;
using System.Linq;
using Cofra.AbstractIL.Common.Statements;

namespace Cofra.AbstractIL.Internal.ControlStructures
{
    public class CommonMethodAsNodeBasedProgram : INodeBasedProgram<int>
    {
        private readonly Common.Types.Method mySource;

        public CommonMethodAsNodeBasedProgram(Common.Types.Method source)
        {
            mySource = source;
        }

        public Statement StatementAt(int position)
        {
            return mySource.Instructions[position].Statement;
        }

        public IEnumerable<int> Transitions(int position)
        {
            return mySource.Instructions[position]
                .Continuation.NextInstructions
                .Select(id => id.Value);
        }

        public IEnumerable<int> GetStarts()
        {
            return mySource.InitialInstructions.NextInstructions.Select(id => id.Value);
        }

        public bool IsFinal(int position)
        {
            return mySource.Instructions[position].IsFinal();
        }
    }
}
