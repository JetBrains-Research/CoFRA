using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Internal.Types.Primaries
{
    [DataContract]
    public struct ResolvedMethodId
    {
        [DataMember]
        public readonly int GlobalId;

        public ResolvedMethodId(int globalId)
        {
            GlobalId = globalId;
        }

        public override bool Equals(object obj)
        {
            return obj is ResolvedMethodId id &&
                   GlobalId == id.GlobalId;
        }

        public override int GetHashCode()
        {
            return -2093202133 + GlobalId.GetHashCode();
        }
    }
}
