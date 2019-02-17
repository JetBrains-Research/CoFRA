using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Cofra.AbstractIL.Internal.Types
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
