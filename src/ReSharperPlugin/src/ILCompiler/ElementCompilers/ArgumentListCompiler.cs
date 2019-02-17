using System;
using System.Collections.Generic;
using System.Linq;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class ArgumentListCompiler : ElementCompiler
    {
        private readonly IArgumentList myArgumentList;

        public ArgumentListCompiler(IArgumentList argumentList, AbstractILCompilerParams myParams) : base(myParams)
        {
            myArgumentList = argumentList;
        }

        public override ICompilationResult GetResult()
        {
            var results = new Dictionary<ParameterIndex, IExpressionCompilationResult>();
            foreach (var argument in myArgumentList.ArgumentsEnumerable)
            {
                var matchingParameter = argument.MatchingParameter;
                
                var invocationParamNumber = matchingParameter == null ? 0 :
                    matchingParameter.Element.ContainingParametersOwner.Parameters.IndexOf(matchingParameter.Element);
                IExpressionCompilationResult res;
                if (MyChildToResult[argument] is IExpressionCompilationResult expressionCompilationResult)
                    res = expressionCompilationResult;
                else
                    res = new ExpressionCompilationResult();
                var index = new ParameterIndex(invocationParamNumber);
                // for params
                if (!results.ContainsKey(index))
                    results.Add(index, res);
            }

            var x = new ArgumentListCompilationResult(results);

            return x;
        }
    }
}