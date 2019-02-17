using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.AnalysisSpecific;

namespace Cofra.ReSharperPlugin.ILCompiler.CompilationResults
{
    public interface IExpressionCompilationResult : ICompilationResult
    {
        Reference GetReference();
        ExpressionCompilationResult TryConvertToGetter(AbstractILCompilerParams @params);
        ExpressionCompilationResult TryGetSetter(AbstractILCompilerParams @params, Reference reference);
    }
}