using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Cofra.AbstractIL.Internal.Types
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
