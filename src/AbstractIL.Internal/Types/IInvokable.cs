using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofra.AbstractIL.Internal.Types
{
    public interface IInvokable<out TNode>
    {
        TNode EntryPoint { get; }
        IReadOnlyDictionary<int, ResolvedLocalVariable> Variables { get; }

        void MarkAsInvoked();
    }
}
