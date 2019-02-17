using System.Collections.Generic;
using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Types;
using JetBrains.Annotations;

namespace Cofra.AbstractIL.Common.ControlStructures
{
    public interface IInstructionsContainer
    {
        InstructionBlock GetInstructionBlock();
        bool IsEmpty();
    }

    [DataContract]
    public class InstructionBlock : IInstructionsContainer
    {
        [NotNull] [DataMember] public readonly List<Continuation> Continuations;
        [NotNull] [DataMember] public readonly List<InstructionId> InitialInstructions;
        [NotNull] [DataMember] public readonly List<Instruction> Instructions;
        //public readonly Dictionary<InstructionId,Instruction> IdToInstruction;

        public InstructionBlock(List<InstructionId> initialInstructions, List<Instruction> instructions,
            List<Continuation> continuations)
        {
            Continuations = continuations ?? new List<Continuation>();
            Instructions = instructions ?? new List<Instruction>();
            InitialInstructions = initialInstructions ?? new List<InstructionId>();
        }

        public InstructionBlock(Instruction instruction)
        {
            Continuations = new List<Continuation> {instruction.Continuation};
            Instructions = new List<Instruction> {instruction};
            InitialInstructions = new List<InstructionId> {instruction.Id};
        }

        public InstructionBlock()
        {
            Continuations = new List<Continuation>();
            Instructions = new List<Instruction>();
            InitialInstructions = new List<InstructionId>();
        }

        public bool AreContinuationsEmpty()
        {
            if (Continuations.Count == 0) return true;
            return !Continuations.Exists(cont => cont.NextInstructions.Count > 0);
        }

        public InstructionBlock GetInstructionBlock()
        {
            return this;
        }

        public bool IsEmpty()
        {
            return InitialInstructions.Count == 0;
        }
    }
}