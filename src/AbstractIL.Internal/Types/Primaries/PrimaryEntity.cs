using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Internal.Types.Primaries
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
