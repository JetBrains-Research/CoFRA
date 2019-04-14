using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Types.Ids;

namespace Cofra.AbstractIL.Common.Types
{
    [DataContract]
    public class ClassReference : Reference
    {
        [DataMember] public bool ThisClassReference;
        [DataMember] public ClassId ClassId;

        public ClassReference(ClassId classId, bool thisClassReference = false)
        {
            ClassId = classId;
            ThisClassReference = thisClassReference;
        }

        public override string ToString()
        {
            return $"ClassRef:{ClassId}";
        }
    }
}