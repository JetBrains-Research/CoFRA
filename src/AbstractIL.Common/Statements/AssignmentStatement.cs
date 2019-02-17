using System;
using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Types;

namespace Cofra.AbstractIL.Common.Statements
{
    [DataContract]
    [KnownType(typeof(ReturnStatement))]
    public class AssignmentStatement : Statement
    {
        public AssignmentStatement(Location location, Reference source, Reference target)
            : base(location)
        {
            Source = source;
            Target = target;
        }

        [DataMember] public readonly Reference Source;
        [DataMember] public readonly Reference Target;

        public override StatementType Type => StatementType.Assignment;

        public override string ToString()
        {
            return $"{base.ToString()}: Source:{Source}, Target:{Target}";
        }
    }
}