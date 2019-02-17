using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Types;

namespace Cofra.AbstractIL.Common.Statements
{
    [DataContract]
    public sealed class CatchStatement : Statement
    {
        public CatchStatement(
            Location location,
            InstructionId exceptionThrowerInstruction,
            InstructionId bodyBlockStart,
            InstructionId catchBlockStart)
            : base(location)
        {
            ExceptionThrowerInstruction = exceptionThrowerInstruction;
            BodyBlockStart = new Continuation(bodyBlockStart);
            CatchBlockStart = new Continuation(catchBlockStart);
        }

        [DataMember] public readonly InstructionId ExceptionThrowerInstruction;
        [DataMember] public readonly Continuation BodyBlockStart;
        [DataMember] public readonly Continuation CatchBlockStart;

        public override StatementType Type => StatementType.Catch;
    }
}