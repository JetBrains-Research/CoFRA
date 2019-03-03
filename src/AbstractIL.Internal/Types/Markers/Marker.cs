using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Internal.Types.Primaries;

namespace Cofra.AbstractIL.Internal.Types.Markers
{
    [DataContract]
    [KnownType(typeof(TaintMarker))]
    public abstract class Marker : PrimaryEntity
    {
        public abstract bool IsPropagating { get; }
    }
}
