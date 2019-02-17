using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Types;

namespace Cofra.AbstractIL.Common.Statements
{
    [DataContract]
    public sealed class ExceptionStatement : Statement
    {
        public ExceptionStatement(
            Location location,
            InstructionId parentExceptionThrowerInstruction)
            : base(location)
        {
            ParentExceptionThrowerInstruction = parentExceptionThrowerInstruction;
        }

        [DataMember] public readonly InstructionId ParentExceptionThrowerInstruction;

        public override StatementType Type => StatementType.Exception;
    }
}