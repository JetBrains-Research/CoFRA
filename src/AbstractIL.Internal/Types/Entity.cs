using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Internal.Types.Primaries;
using Cofra.AbstractIL.Internal.Types.Secondaries;

namespace Cofra.AbstractIL.Internal.Types
{
    [DataContract]
    [KnownType(typeof(PrimaryEntity))]
    [KnownType(typeof(SecondaryEntity))]
    public abstract class Entity
    {
    }
}
