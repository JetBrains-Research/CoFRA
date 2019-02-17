using System.Linq;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Statements;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class AnonymousFunctionCompiler : ExpressionCompiler
    {
        private IAnonymousFunctionExpression myAnonymousFunctionExpression;
        public AnonymousFunctionCompiler(IAnonymousFunctionExpression anonymousFunction, AbstractILCompilerParams @params) : base(@params)
        {
            myAnonymousFunctionExpression = anonymousFunction;
            myReference = MyParams.CreateAnonymousFunction(myAnonymousFunctionExpression.DeclaredElement);
        }

        public override ICompilationResult GetResult()
        {
            var declaredElemMethod = myAnonymousFunctionExpression.DeclaredElement;
            if (declaredElemMethod == null) return new ElementCompilationResult();

            var results = MyChildToResult.Where(x => !(x.Key is ILambdaSignature)).Select(x => x.Value).ToList();
            
            var instructionBlock = GetInstructionsConnectedSequentially(results);

            var method = MyParams.GetCurrentMethod();
            
            method.FillWithInstructions(instructionBlock);
            MyParams.FinishCurrentMethod();
            return new ExpressionCompilationResult(new InstructionBlock(), GetLocation(myAnonymousFunctionExpression), reference: myReference);
        }
    }
}