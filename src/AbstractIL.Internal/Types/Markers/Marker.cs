using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Internal.Types.Primaries;

namespace Cofra.AbstractIL.Internal.Types.Markers
{
    public abstract class Marker : PrimaryEntity
    {
        public abstract bool IsPropagating { get; }
    }
}
