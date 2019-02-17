using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Cofra.AbstractIL.Internal.Types
{
    [DataContract]
    public struct ResolvedFullMethodId
    {
        [DataMember]
        public readonly ResolvedClassId ClassId;

        [DataMember]
        public readonly ResolvedMethodId MethodId;

        public ResolvedFullMethodId(ResolvedClassId classId, ResolvedMethodId methodId)
        {
            ClassId = classId;
            MethodId = methodId;
        }

        public override bool Equals(object obj)
        {
            return obj is ResolvedFullMethodId id &&
                   EqualityComparer<ResolvedClassId>.Default.Equals(ClassId, id.ClassId) &&
                   EqualityComparer<ResolvedMethodId>.Default.Equals(MethodId, id.MethodId);
        }

        public override int GetHashCode()
        {
            var hashCode = -863107345;
            hashCode = hashCode * -1521134295 + EqualityComparer<ResolvedClassId>.Default.GetHashCode(ClassId);
            hashCode = hashCode * -1521134295 + EqualityComparer<ResolvedMethodId>.Default.GetHashCode(MethodId);
            return hashCode;
        }
    }
}
