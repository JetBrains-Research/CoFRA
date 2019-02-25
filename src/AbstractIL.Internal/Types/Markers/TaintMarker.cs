using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofra.AbstractIL.Internal.Types.Markers
{
    public sealed class TaintMarker : Marker
    {
        public static TaintMarker Instance { get; } = new TaintMarker();

        public override bool IsPropagating => true;

        private TaintMarker()
        {
        }

        public override bool Equals(object another)
        {
            return another is TaintMarker;
        }

        private bool Equals(TaintMarker other)
        {
            return true;
        }

        public override int GetHashCode()
        {
            return 983752834;
        }
    }
}
