using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Internal.Types;
using Cofra.AbstractIL.Internal.Types.Secondaries;

namespace Cofra.AbstractIL.Internal.Statements
{
    [DataContract]
    public class ResolvedAssignmentStatement : InternalStatement
    {
        [DataMember]
        public readonly Entity Source;

        [DataMember]
        public readonly SecondaryEntity Target;

        [DataMember]
        public readonly bool TargetReferencedByThis;

        public ResolvedAssignmentStatement(Location location, Entity source, SecondaryEntity target, bool targetReferencedByThis = false)
            : base(location)
        {
            Source = source;
            Target = target;
            TargetReferencedByThis = targetReferencedByThis;
        }

        public override InternalStatementType InternalType => 
            InternalStatementType.ResolvedAssignment;
    }
}
