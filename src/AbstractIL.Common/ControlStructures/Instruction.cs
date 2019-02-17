using System;
using System.Runtime.Serialization;
using System.Text;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Common.Types;

namespace Cofra.AbstractIL.Common.ControlStructures
{
    [DataContract]
    public sealed class Instruction : IInstructionsContainer
    {
        [DataMember] public readonly Statement Statement;
        [DataMember] public readonly Continuation Continuation;
        
        public InstructionId Id { get; }
        private InstructionBlock myInstructionBlock;
        
        public Instruction(Statement statement, InstructionId id, Continuation continuation = null)
        {
            Id = id;
            Statement = statement ?? throw new Exception("Instruction statement can't be null");
            Continuation = continuation ?? new Continuation();
        }

        public InstructionBlock GetInstructionBlock()
        {
            return myInstructionBlock ?? (myInstructionBlock = new InstructionBlock(this));
        }

        public bool IsEmpty()
        {
            return false;
        }

        public bool IsFinal()
        {
            return Continuation.IsEmpty();            
        }
        
        public override string ToString()
        {
            return ($"{Id} --> {Continuation}::{Statement}");
        }
    }
}