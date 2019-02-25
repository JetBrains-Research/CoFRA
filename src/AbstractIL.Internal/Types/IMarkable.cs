using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Internal.Types.Markers;

namespace Cofra.AbstractIL.Internal.Types
{
    public interface IMarkable
    {
        IEnumerable<Marker> Markers { get; }

        void Mark(Marker marker);
        bool RemoveMarker(Marker marker);
        bool MarkedWith(Marker marker);
    }
}
