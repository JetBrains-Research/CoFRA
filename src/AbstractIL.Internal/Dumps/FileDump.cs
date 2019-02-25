using System.Collections.Generic;
using System.Runtime.Serialization;
using Cofra.AbstractIL.Internal.Types;
using Cofra.AbstractIL.Internal.Types.Primaries;

namespace Cofra.AbstractIL.Internal.Dumps
{
    [DataContract]
    public struct FileDump
    {
        [DataMember]
        public readonly List<ResolvedFullMethodId> MethodIds;

        public FileDump(IEnumerable<ResolvedFullMethodId> methodIds)
        {
            MethodIds = new List<ResolvedFullMethodId>(methodIds);
        }
    }
}
