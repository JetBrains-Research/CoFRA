using System;
using System.Collections.Generic;
using System.Linq;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.AnalysisSpecific;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.Annotations;
using JetBrains.Platform.MsBuildTask.Utils;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class ExpressionCompiler : ElementCompiler
    {
        protected Reference myReference;
        public ExpressionCompiler(AbstractILCompilerParams @params) : base(@params)
        {
        }

        public override ICompilationResult GetResult()
        {
            var instructionBlock = GetInstructionsConnectedSequentially(MyResults);
            var reference = GetReferenceFromResults(MyResults);
            var withOutGetter = new ExpressionCompilationResult(instructionBlock, null, reference);
            return withOutGetter.TryConvertToGetter(MyParams);
        }

        protected Reference GetReferenceFromResults(IEnumerable<ICompilationResult> results)
        {
            var refs = results
                .Where(x => x is IExpressionCompilationResult)
                .Cast<IExpressionCompilationResult>()
                .Select(x => x/*.TryConvertToGetter(MyParams)*/.GetReference())
                .Where(x => x != null).ToList();
            return refs.IsEmpty() ? null : refs.Last();
        }
        
        // TODO: out parameters
        protected Dictionary<ParameterIndex, Reference> GetVariablePairsFromInvoked(IPrimaryExpression invokedExpression)
        {
            var vars = new Dictionary<ParameterIndex, Reference>();
            vars.Add(ParameterIndex.ReturnValueIndex, MyParams.LocalVariableIndexer.GetNextVariable());
            switch (invokedExpression)
            {
                case IMethod method:
                    
//                    for (int i = 0; i < method.Parameters.Count; i++)
//                    {
//                        if (method.Parameters[i].Kind == ParameterKind.OUTPUT)
//                        {
//                            var name = method.Parameters[i].ShortName;
//                            vars.Add(MyParams.LocalVariableIndexer.GetNextVariable());
//                        }
//                    }
//
//                    method.Parameters
                    break;
                case ILocalFunctionDeclaration localFunction:
                    break;
            }
            
            return vars;
        }
    }
}