using System.Collections.Generic;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class IfStatementCompiler : ElementCompiler
    {
        private readonly IIfStatement myIfStatement;

        public IfStatementCompiler(IIfStatement ifStatement, AbstractILCompilerParams @params) : base(@params)
        {
            myIfStatement = ifStatement;
        }

        public override ICompilationResult GetResult()
        {
            var conditionResult = myIfStatement.Condition != null ? MyChildToResult[myIfStatement.Condition] : new ElementCompilationResult();
            var thenResult = myIfStatement.Then != null ? MyChildToResult[myIfStatement.Then] : null;
            var elseResult = myIfStatement.Else != null ? MyChildToResult[myIfStatement.Else] : null;

            var newInstructionBlock = ConnectOneWithManyResultsToInstructionBlock(conditionResult,
                new List<ICompilationResult> {thenResult, elseResult});

            return new ElementCompilationResult(newInstructionBlock);
        }
    }
}