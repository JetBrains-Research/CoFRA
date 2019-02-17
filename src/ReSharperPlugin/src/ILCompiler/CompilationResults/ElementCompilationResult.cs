using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Types;
using JetBrains.Util;

namespace Cofra.ReSharperPlugin.ILCompiler.CompilationResults
{
    public class ElementCompilationResult : ICompilationResult
    {
        private readonly InstructionBlock _instructionBlock;
//        public readonly List<int> InitialInstructions;
//        public readonly List<Instruction> Instructions;
//        public readonly List<Continuation> Continuations;

        public ElementCompilationResult(List<InstructionId> initialInstructions, List<Instruction> instructions,
            List<Continuation> continuations)
        {
            _instructionBlock = new InstructionBlock(initialInstructions, instructions, continuations);
        }

        public ElementCompilationResult(InstructionBlock instructionBlock)
        {
            _instructionBlock = instructionBlock ?? new InstructionBlock();
        }

        public ElementCompilationResult()
        {
            _instructionBlock = new InstructionBlock();
        }

        public InstructionBlock GetInstructionBlock()
        {
            return _instructionBlock;
        }

        public bool IsEmpty()
        {
            return GetInstructionBlock().InitialInstructions.IsEmpty();
        }
    }
}