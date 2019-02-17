using System.Collections.Generic;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.Ids;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.CompilationResults
{
    public class ArgumentListCompilationResult : ElementCompilationResult
    {
        public readonly Dictionary<ParameterIndex, IExpressionCompilationResult> ArgumentsResults;
        private IArgumentList list;

        public ArgumentListCompilationResult(Dictionary<ParameterIndex, IExpressionCompilationResult> results)
        {
            ArgumentsResults = results;
        }
    }
}