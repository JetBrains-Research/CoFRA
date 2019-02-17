//using System.Threading;
//using Cofra.AbstractIL.Common.ControlStructures;
//using Cofra.AbstractIL.Common.Types;
//using Cofra.AbstractIL.Common.Types.AnalysisSpecific;
//using JetBrains.ReSharper.Psi;
//using JetBrains.ReSharper.Psi.CSharp.Tree;
//using JetBrains.ReSharper.Psi.Util;
//
//namespace ReSharper.IA.Plugin.ILCompiler.CompilationResults
//{
//    public defaultType ReferenceExpressionCompilationResult : ExpressionCompilationResult
//    {
//        public ReferenceExpressionCompilationResult(InstructionBlock instructionBlock, LockType lockType = LockType.None,
//            Reference reference = null) : base(instructionBlock,lockType,reference) 
//        {
////            myReferenceExpression = referenceExpression;
////            DeclaredElement = referenceExpression.Reference.Resolve().DeclaredElement;
////
////            switch (DeclaredElement)
////            {
////                case IParameter currentFunParameter
////                    when currentFunParameter.IsDelegateInvokeMethod() || currentFunParameter.Type.IsAction():
////                {
////                    var currentFunParamNumber =
////                        currentFunParameter.ContainingParametersOwner?.Parameters.IndexOf(currentFunParameter);
////                    Reference = currentFunParamNumber == null
////                        ? null
////                        : new LocalVariableReference(currentFunParamNumber.Value);
////                    break;
////                }
////                case IMethod referencedDelegateMethod
////                    when !CompilerUtils.NeedToSkipInvocation(referencedDelegateMethod):
////                {
////                    Reference = new GlobalFunction(CompilerUtils.GetMethodId(referencedDelegateMethod));
////                    break;
////                }
////            }
//        }
//    }
//}