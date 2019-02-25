using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Internal.Types.Secondaries
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
