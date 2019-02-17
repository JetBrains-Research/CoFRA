using Cofra.AbstractIL.Common;
using Cofra.AbstractIL.Common.Types;

namespace Cofra.ReSharperPlugin.ILCompiler.CompilationResults
{
    public class MethodDeclarationCompilationResult : ElementCompilationResult
    {
        public Method Method { get; }
        
        public MethodDeclarationCompilationResult(Method method)
        {
            Method = method;
        }
    }
}