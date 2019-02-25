using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Internal.Types.Primaries;

namespace Cofra.AbstractIL.Internal.Types
{
    public interface IMethodHolder<TNode>
    {
        ResolvedMethod<TNode> FindMethodInFullHierarchy(ResolvedMethodId id);
        ResolvedMethod<TNode> FindLocalMethod(ResolvedMethodId id);

        IEnumerable<ResolvedMethod<TNode>> CollectAllInternalMethods();
        IEnumerable<(ResolvedMethodId, ResolvedMethod<TNode>)> CollectInternalMethods();

        void AddMethod(ResolvedMethodId id, ResolvedMethod<TNode> method);
        bool RemoveMethod(ResolvedMethodId id);
    }
}
