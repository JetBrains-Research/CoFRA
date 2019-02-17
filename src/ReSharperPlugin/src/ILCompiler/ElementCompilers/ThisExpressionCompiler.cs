using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Types.AnalysisSpecific;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class ThisExpressionCompiler : ExpressionCompiler
    {
        private IThisExpression myThisExpression;
        public ThisExpressionCompiler(IThisExpression thisExpression, AbstractILCompilerParams @params) : base(@params)
        {
            myThisExpression = thisExpression;
        }

        public override ICompilationResult GetResult()
        {
            return new ExpressionCompilationResult(new InstructionBlock(), GetLocation(myThisExpression), reference: MyParams.GetCurrentClassReference());
        }
    }
}