using System.Collections.Generic;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Common.Types;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class ReturnStatementCompiler : ElementCompiler
    {
        private IReturnStatement myReturnStatement;
        public ReturnStatementCompiler(IReturnStatement node, AbstractILCompilerParams myParams) : base(myParams)
        {
            myReturnStatement = node;
        }

        public override ICompilationResult GetResult()
        {
            var baseResult = base.GetResult();
            var resultExpression = myReturnStatement.Value;
            if (resultExpression == null || myReturnStatement.Value == null) return baseResult;

            var result = MyChildToResult[myReturnStatement.Value];
            var resWithGetter = (result as IExpressionCompilationResult).TryConvertToGetter(MyParams);
            
            var reference = resWithGetter.GetReference();

            if (reference == null)
            {
                return new ElementCompilationResult();
            }
            var assignment = new ReturnStatement(GetLocation(myReturnStatement), reference);
            var instruction = new Instruction(assignment, MyParams.GetNewInstructionId());

            return new ElementCompilationResult(GetInstructionsConnectedSequentially(new List<IInstructionsContainer>{ resWithGetter, instruction}));
        }
    }
}