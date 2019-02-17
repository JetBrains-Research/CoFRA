using System.Linq;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Types;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class SwitchBlockCompiler : ElementCompiler
    {
        public SwitchBlockCompiler(ISwitchBlock switchBlock, AbstractILCompilerParams @params) : base(@params)
        {
        }

        public override ICompilationResult GetResult()
        {
            var instructionBlock = new InstructionBlock();

            foreach (var result in MyResults)
            {
                var resultBlock = result.GetInstructionBlock();
                instructionBlock.InitialInstructions.AddRange(resultBlock.InitialInstructions);
                instructionBlock.Instructions.AddRange(resultBlock.Instructions);
                instructionBlock.Continuations.AddRange(resultBlock.Continuations);
            }
            
            return new ElementCompilationResult(instructionBlock);
        }
    }
}