using System.Collections.Generic;
using System.Linq;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Statements;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class ArrowExpressionClauseCompiler : ExpressionCompiler
    {
        private IArrowExpressionClause myArrowExpressionClause;
        private readonly bool IsMethod; 

        private string Str => "12" + "123";
        public ArrowExpressionClauseCompiler(IArrowExpressionClause arrowExpressionClause, AbstractILCompilerParams @params) : base(@params)
        {
            myArrowExpressionClause = arrowExpressionClause;
            switch (myArrowExpressionClause.Parent)
            {
                case IPropertyDeclaration propertyDeclaration:
                    var name = $"get_{propertyDeclaration.DeclaredName}";
                    MyParams.CreateMethod(null, name);
                    IsMethod = true;
                    break;
                case IMethodDeclaration _:
                case ILocalFunctionDeclaration _:
                case IAccessorDeclaration _:
                case IConstructorDeclaration _:
                case IIndexerDeclaration _:
                    IsMethod = false;
                    break;
                default:
                    throw MyParams.CreateException(
                        $"parent of ArrowExpression is {myArrowExpressionClause.Parent?.GetType()}");
            }
        }

        public override ICompilationResult GetResult()
        {
            if (!(base.GetResult() is ExpressionCompilationResult resultWithGetter))
                return new ElementCompilationResult();

            var reference = resultWithGetter.GetReference();

            if (reference == null)
                return new ElementCompilationResult();
            var assignment = new ReturnStatement(GetLocation(myArrowExpressionClause), reference);
            var instruction = new Instruction(assignment, MyParams.GetNewInstructionId());
            
            var instructionBlock = GetInstructionsConnectedSequentially(new List<IInstructionsContainer>{ resultWithGetter, instruction});
            if (IsMethod)
            {
                MyParams.GetCurrentMethod().FillWithInstructions(instructionBlock);
                MyParams.FinishCurrentMethod();
                return new ElementCompilationResult();
            }
            return new ExpressionCompilationResult(instructionBlock, GetLocation(myArrowExpressionClause), reference: myReference);
        }
    }
}