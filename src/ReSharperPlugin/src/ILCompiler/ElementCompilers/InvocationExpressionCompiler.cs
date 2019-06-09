using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.AbstractIL.Common.Types.InvocationTargets;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.Collections;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class InvocationExpressionCompiler : ExpressionCompiler
    {
        private readonly IInvocationExpression myInvocationExpression;
        private List<ElementCompilationResult> myUnusedResults = new List<ElementCompilationResult>();
        public Dictionary<int, Reference> ReturnedValues;
        public List<Statement> Statements;

        public InvocationExpressionCompiler(IInvocationExpression invocationExpression,
            AbstractILCompilerParams @params) :
            base(@params)
        {
            myInvocationExpression = invocationExpression;
        }

        public override ICompilationResult GetResult()
        {
            var location = GetLocation(myInvocationExpression);

            var argumentListResult =
                MyChildToResult[myInvocationExpression.ArgumentList] as ArgumentListCompilationResult;
            var invokedExpressionCompilationResult = MyChildToResult[myInvocationExpression.InvokedExpression];
            var delegateParameters = new Dictionary<int, Reference>();
            var arguments = new Dictionary<ParameterIndex, Reference>();

            foreach (var (inParameter, result) in argumentListResult.ArgumentsResults)
            {
                // call getter
                var passedVariable = result.GetReference();

                if (passedVariable != null) arguments.Add(inParameter, passedVariable);
            }

            var argumentsInstructionBlock = GetInstructionsConnectedSequentially(argumentListResult.ArgumentsResults.Select(kv => kv.Value));

            if (!(invokedExpressionCompilationResult is IExpressionCompilationResult expressionCompilationResult))
                throw MyParams.CreateException("invoked expression result in not an expression compilation result");
            var returnedValues = GetVariablePairsFromInvoked(myInvocationExpression.InvokedExpression);
            // call getter
            var expressionResult = expressionCompilationResult.TryConvertToGetter(MyParams);
            var invokedVar = expressionResult.GetReference();

            InvocationStatement fakeInvocation = null;
            var needToInvoke = true;
            if (invokedVar != null)
            {
                InvocationTarget invocationTarget;
                switch (invokedVar)
                {
                    case LocalVariableReference localVariableReference:
                        invocationTarget = new LocalVariableTarget(localVariableReference);
                        break;
                    case LocalFunctionReference localFunctionReference:
                        invocationTarget = new LocalFunctionTarget(localFunctionReference);
                        MyParams.CheckExistenceOfLocalFunction(localFunctionReference);
                        break;
                    case ClassMethodReference classMethodReference:
                        invocationTarget = new ClassMethodTarget(classMethodReference);
                        if (myInvocationExpression.Reference == null)
                        {
                            throw MyParams.CreateException("invocation reference is null");
                        }
                        if (myInvocationExpression.Reference.Resolve().DeclaredElement == null ||
                            myInvocationExpression.Reference.Resolve().DeclaredElement.GetType().ToString() != "JetBrains.ReSharper.Psi.CSharp.Impl.DeclaredElement.CSharpMethod")
                        {
                            needToInvoke = false;
                            if (arguments.TryGetValue(new ParameterIndex(0), out var reference)
                                && reference is LocalFunctionReference funRef)
                            {
                                fakeInvocation = new InvocationStatement(location,
                                        new LocalFunctionTarget(funRef), 
                                        new Dictionary<ParameterIndex, Reference>(),
                                        new Dictionary<ParameterIndex, Reference>());
                            }
                        }
                        break;
                    case ClassFieldReference classFieldReference:
                        invocationTarget = new ClassFieldTarget(classFieldReference);
                        break;
                    case ClassPropertyReference classPropertyReference:
                        Trace.Assert(false, "invocation target is property");
                        invocationTarget = new ClassPropertyTarget(classPropertyReference);
                        break;
                    default:
                        throw MyParams.CreateException($"{invokedVar.GetType()}: unsupported reference type to invoke");
                }

//                if (invocationTarget is ClassMethodTarget classMethodTarget &&
//                    CompilerUtils.IsAddToCollectionMethod(classMethodTarget))
//                {
//                    myInvocationExpression.InvokedExpression.
//                    var statement = new InvocationStatement(location,
//                        invocationTarget,
//                        arguments,
//                        returnedValues);
//                    
//                }
                myReference = returnedValues[ParameterIndex.ReturnValueIndex];
                var summaryBlock = GetInstructionsConnectedSequentially(new List<IInstructionsContainer>
                    {expressionResult, argumentsInstructionBlock});
                
                if (needToInvoke || fakeInvocation != null)
                {
                    var statement = fakeInvocation ?? new InvocationStatement(location,
                                        invocationTarget,
                                        arguments,
                                        returnedValues);

                    var invocationInstruction = new Instruction(statement, MyParams.GetNewInstructionId());
                    
                    summaryBlock = GetInstructionsConnectedSequentially(new List<IInstructionsContainer>
                        {summaryBlock, invocationInstruction});
                }
                
                return new ExpressionCompilationResult(summaryBlock, location, reference: myReference);
            }
            
            return new ExpressionCompilationResult(expressionResult.GetInstructionBlock(), location, reference : myReference);
        }

        
    }
}