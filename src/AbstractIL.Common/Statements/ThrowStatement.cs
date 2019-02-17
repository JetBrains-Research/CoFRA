using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Types;

namespace Cofra.AbstractIL.Common.Statements
{
    [DataContract]
    public sealed class ThrowStatement : Statement
    {
        public ThrowStatement(
            Location location,
            InstructionId exceptionThrowerInstruction)
            : base(location)
        {
            ExceptionThrowerInstruction = exceptionThrowerInstruction;
        }

        [DataMember] public readonly InstructionId ExceptionThrowerInstruction;

        public override StatementType Type => StatementType.Throw;
    }
}