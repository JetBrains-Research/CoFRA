using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Internal.Types
{
    [DataContract]
    public sealed class ResolvedLocalFunctionReference<TNode> : PrimaryEntity, IInvokable<TNode>
    {
        [DataMember]
        public readonly ResolvedMethodId MethodId;

        public ResolvedMethod<TNode> Method { get; set; }

        public ResolvedLocalFunctionReference(ResolvedMethodId methodId)
        {
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
