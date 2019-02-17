using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Types.AnalysisSpecific;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class BaseExpressionCompiler : ExpressionCompiler
    {
        private readonly IBaseExpression myBaseExpression;
        public BaseExpressionCompiler(IBaseExpression baseExpression, AbstractILCompilerParams @params) : base(@params)
        {
            myBaseExpression = baseExpression;
        }

        public override ICompilationResult GetResult()
        {
            var reference = MyParams.GetBaseOfCurrentClass();
            return new ExpressionCompilationResult(new InstructionBlock(), GetLocation(myBaseExpression), reference : reference);
        }
    }
}