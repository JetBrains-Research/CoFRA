using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Statements;
using QuickGraph;

namespace Cofra.AbstractIL.Internal.ControlStructures
{
    [DataContract]
    [KnownType(typeof(OperationEdge<int>))]
    public class OperationEdge<TNode> : Operation<TNode>, IEdge<TNode>
    {
        [DataMember(Name = "Source")] 
        private readonly TNode mySource;

        [DataMember(Name = "Target")] 
        private readonly TNode myTarget;

        [DataMember(Name = "Statement")] 
        private readonly Statement myStatement;

        public TNode Source => mySource;
        public override TNode Target => myTarget;
        public override Statement Statement => myStatement;

        public OperationEdge(TNode source, Statement statement, TNode target)
        {
            mySource = source;
            myTarget = target;
            myStatement = statement;
        }

        public OperationEdge(TNode source, Operation<TNode> operation)
            : this(source, operation.Statement, operation.Target)
        {
        }

        public override bool Equals(object obj)
        {
            return obj is OperationEdge<TNode> edge &&
                   Source.Equals(edge.Source) &&
                   Target.Equals(edge.Target) &&
                   Statement.Equals(edge.Statement);
        }

        public override int GetHashCode()
        {
            return 924162744 +
                   Source.GetHashCode() *
                   Target.GetHashCode() *
                   Statement.GetHashCode();
        }
    }
}
