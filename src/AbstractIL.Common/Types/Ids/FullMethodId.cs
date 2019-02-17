using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Common.Types.Ids
{
    [DataContract]
    public sealed class FullMethodId
    {
        [DataMember]
        public readonly ClassId ClassId;

        [DataMember]
        public readonly MethodId MethodId;

        public FullMethodId(ClassId classId, MethodId methodId)
        {
            ClassId = classId;
            MethodId = methodId;
        }
    }
}
