using System.Collections.Generic;
using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Types;

namespace Cofra.AbstractIL.Internal.Types
{
    [DataContract]
    public sealed class ResolvedLocalVariable : SecondaryEntity
    {
        [DataMember]
        public readonly int LocalId;

        [DataMember] 
        public int DefaultClassType = -1;

        public ResolvedLocalVariable(int localId, int defaultType = -1)
        {
            LocalId = localId;
            DefaultClassType = defaultType;
        }
    }
}
