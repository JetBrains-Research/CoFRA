using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Types;

namespace Cofra.AbstractIL.Internal.Types
{
    [DataContract]
    public sealed class ResolvedClassId : PrimaryEntity 
    {
        [DataMember]
        public readonly int GlobalId;

        public ResolvedClassId(int globalId)
        {
            GlobalId = globalId;
        }

        public override bool Equals(object obj)
        {
            return obj is ResolvedClassId id &&
                   GlobalId == id.GlobalId;
        }

        public override int GetHashCode()
        {
            return -2093202133 + GlobalId.GetHashCode();
        }
    }
}
