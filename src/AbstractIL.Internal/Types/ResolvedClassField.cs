using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Cofra.AbstractIL.Internal.Types
{
    [DataContract]
    public sealed class ResolvedClassField : SecondaryEntity
    {
        [DataMember]
        public readonly ResolvedClassId ClassId;

        [DataMember]
        public readonly int VariableId;

        [DataMember]
        public int DefaultClassType;

        public ResolvedClassField(ResolvedClassId classId, int variableId, int defaultClassType = -1)
        {
            ClassId = classId;
            VariableId = variableId;
            DefaultClassType = defaultClassType;
        }

        public override bool PropagateUp(SecondaryEntity target)
        {
            return PropagateForward(target);
        }

        public override bool PropagateDown(SecondaryEntity target)
        {
            return PropagateForward(target);
        }
    }
}
