using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Types;

namespace Cofra.AbstractIL.Common.Statements
{
    [DataContract]
    public sealed class FinallyStatement : Statement
    {
        public FinallyStatement(
            Location location,
            InstructionId bodyStart,
            InstructionId finallyBlockStart)
            : base(location)
        {
            BodyStart = new Continuation(bodyStart);
            FinallyBlockStart = new Continuation(finallyBlockStart);
        }

        [DataMember] public readonly Continuation BodyStart;
        [DataMember] public readonly Continuation FinallyBlockStart;

        public override StatementType Type => StatementType.Finally;
    }
}