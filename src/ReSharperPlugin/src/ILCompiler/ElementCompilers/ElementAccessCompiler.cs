using System;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class ElementAccessCompiler : ExpressionCompiler
    {
        private IElementAccessExpression myElementAccessExpression;
        
        public ElementAccessCompiler(IElementAccessExpression elementAccessExpression,  AbstractILCompilerParams @params) : base(@params)
        {
            myElementAccessExpression = elementAccessExpression;
        }

        public override ICompilationResult GetResult()
        {
            if (MyChildToResult[myElementAccessExpression.Operand] is ExpressionCompilationResult
                expressionCompilationResult)
            {
                var argumentsResults = MyChildToResult[myElementAccessExpression.ArgumentList];
                var argumentListResult = argumentsResults as ArgumentListCompilationResult;
                var singleArgument = argumentListResult.ArgumentsResults[new ParameterIndex(0)];

                var argumentBlock = singleArgument.TryConvertToGetter(MyParams);
                var argumentReference = argumentBlock.GetReference();
                // call getter
                
                var operandBlock = expressionCompilationResult.TryConvertToGetter(MyParams);
                var operandReference = operandBlock.GetReference();
                if (operandReference == null)
                    throw MyParams.CreateException($"null reference result in element access target");
                
                return new ElementAccessCompilationResult(
                    GetInstructionsConnectedSequentially(new []{argumentBlock, operandBlock}),
                    argumentReference, GetLocation(myElementAccessExpression), operandReference);
            }
            return base.GetResult();
        }
    }
}