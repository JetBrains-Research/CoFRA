using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Internal.Types;

namespace Cofra.AbstractIL.Internal.Statements
{
    [DataContract]
    public class ResolvedAssignmentStatement : InternalStatement
    {
        [DataMember]
        public readonly Entity Source;

        [DataMember]
        public readonly SecondaryEntity Target;

        public ResolvedAssignmentStatement(Location location, Entity source, SecondaryEntity target) 
            : base(location)
        {
            Source = source;
            Target = target;
        }

        public override InternalStatementType InternalType => 
            InternalStatementType.ResolvedAssignment;
    }
}
