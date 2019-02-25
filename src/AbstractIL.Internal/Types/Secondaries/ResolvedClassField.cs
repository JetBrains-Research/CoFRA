using System.Collections.Generic;
using System.Runtime.Serialization;
using Cofra.AbstractIL.Internal.Types.Markers;
using Cofra.AbstractIL.Internal.Types.Primaries;

namespace Cofra.AbstractIL.Internal.Types.Secondaries
{
    [DataContract]
    public sealed class ResolvedClassField : SecondaryEntity, IMarkable
    {
        [DataMember] 
        private HashSet<Marker> myMarkers; 

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
            myMarkers = new HashSet<Marker>();
        }

        public override bool PropagateUp(SecondaryEntity target)
        {
            return PropagateForward(target);
        }

        public override bool PropagateDown(SecondaryEntity target)
        {
            return PropagateForward(target);
        }

        public IEnumerable<Marker> Markers => myMarkers;

        public void Mark(Marker marker)
        {
            myMarkers.Add(marker);
        }

        public bool RemoveMarker(Marker marker)
        {
            return myMarkers.Remove(marker);
        }

        public bool MarkedWith(Marker marker)
        {
            return myMarkers.Contains(marker);
        }
    }
}
