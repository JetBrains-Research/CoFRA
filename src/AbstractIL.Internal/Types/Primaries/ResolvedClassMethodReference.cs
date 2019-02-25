using System.Collections.Generic;
using System.Runtime.Serialization;
using Cofra.AbstractIL.Internal.Types.Secondaries;

namespace Cofra.AbstractIL.Internal.Types.Primaries
{
    [DataContract]
    public sealed class ResolvedClassMethodReference<TNode> : PrimaryEntity, IInvokable<TNode>
    {
        [DataMember] 
        public readonly ResolvedClassId OwnerId;

        [DataMember]
        public readonly ResolvedMethodId MethodId;

        public ResolvedMethod<TNode> Method { get; set; }

        public ResolvedClassMethodReference(ResolvedClassId ownerId, ResolvedMethodId methodId)
        {
            OwnerId = ownerId;
            MethodId = methodId;
        }

        public TNode EntryPoint => Method.Start;
        public IReadOnlyDictionary<int, ResolvedLocalVariable> Variables => Method.Variables;

        public void MarkAsInvoked()
        {
            Method.MarkAsInvoked();
        }
    }
}
