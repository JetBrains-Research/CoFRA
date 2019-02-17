using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Cofra.AbstractIL.Internal.Types
{
    [DataContract]
    [KnownType(typeof(ResolvedClassId))]
    [KnownType(typeof(ResolvedMethod<int>))]
    [KnownType(typeof(ResolvedClassMethodReference<int>))]
    [KnownType(typeof(ResolvedLocalFunctionReference<int>))]
    public abstract class PrimaryEntity : Entity
    {
    }
}
