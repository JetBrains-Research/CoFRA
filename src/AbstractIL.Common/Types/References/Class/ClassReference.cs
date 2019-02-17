using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Types.Ids;

namespace Cofra.AbstractIL.Common.Types
{
    [DataContract]
    public class ClassReference : Reference
    {
        [DataMember] public ClassId ClassId;

        public ClassReference(ClassId classId)
        {
            ClassId = classId;
        }

        public override string ToString()
        {
            return $"ClassRef:{ClassId}";
        }
    }
}