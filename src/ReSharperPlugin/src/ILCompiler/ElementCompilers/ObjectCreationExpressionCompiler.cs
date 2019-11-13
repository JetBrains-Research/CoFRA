using System;
using System.Collections.Generic;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.AbstractIL.Common.Types.InvocationTargets;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.Collections;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Impl.Special;
using JetBrains.ReSharper.Psi.Resolve;

using JetBrains.Util;
using IObjectCreationExpression = JetBrains.ReSharper.Psi.CSharp.Tree.IObjectCreationExpression;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class ObjectCreationExpressionCompiler : ExpressionCompiler
    {
        private readonly IObjectCreationExpression myObjectCreationExpression;
        public ObjectCreationExpressionCompiler(IObjectCreationExpression objectCreationExpression, AbstractILCompilerParams @params) : base(@params)
        {
            myObjectCreationExpression = objectCreationExpression;
        }
        
        public override ICompilationResult GetResult()
        {
            IReference reference;
            var location = GetLocation(myObjectCreationExpression);
            switch (myObjectCreationExpression.TypeUsage)
            {
                case IUserTypeUsage userDeclaredTypeUsage:
                    reference = userDeclaredTypeUsage.ScalarTypeName.Reference;
                    break;
                case IPredefinedTypeUsage predefinedDeclaredTypeUsage:
                    reference = predefinedDeclaredTypeUsage.ScalarPredefinedTypeName.Reference;
                    break;
                case IDynamicTypeUsage dynamicDeclaredTypeUsage:
                    throw MyParams.CreateException("found dynamicDeclaredTypeUsage");
                case ITupleTypeUsage tupleDeclaredTypeUsage:
                    throw MyParams.CreateException("found tupleDeclaredTypeUsage");
                    break;
                default:
                    throw MyParams.CreateException($"{myObjectCreationExpression.TypeUsage.GetType()} is unexpected type");
            }

            var declaredElement = reference.Resolve().DeclaredElement;

            if (declaredElement == null)
                return new ExpressionCompilationResult(new InstructionBlock(), location);
                
            if (!(declaredElement is ITypeElement typeElement))
                throw MyParams.CreateException("In object creation declared element " + declaredElement.GetElementType().PresentableName + " is not a type element");

            var classRef = typeElement.GetClassReference();

            var constructor = myObjectCreationExpression.ConstructorReference.Resolve().DeclaredElement as IConstructor;

            if (constructor == null)
            {
                return new ExpressionCompilationResult(new InstructionBlock(), location, reference: classRef);
            }
            
            var invokedFun = new ClassMethodTarget(new ClassMethodReference(classRef,constructor.GetNameWithHash()));

            
            var passedParameters = new Dictionary<ParameterIndex, Reference>();

            if (myObjectCreationExpression.ArgumentList != null)
            {
                if (MyChildToResult[myObjectCreationExpression.ArgumentList] is ArgumentListCompilationResult
                    argumentListResult)
                {
                    foreach (var (inParameter, result) in argumentListResult.ArgumentsResults)
                    {
                        // call getter
                        var passedVariable = result.GetReference();

                        if (passedVariable != null) passedParameters.Add(inParameter, passedVariable);
                    }
                }
            }
            
            var returnedValues = GetVariablePairsFromInvoked(myObjectCreationExpression);

            var statement = new InvocationStatement(location, invokedFun, passedParameters, returnedValues, true);
            
            var instruction = new Instruction(statement, MyParams.GetNewInstructionId());

            var returned = (LocalVariableReference) statement.ReturnedValues[new ParameterIndex(-1)];
            returned.DefaultType = classRef.ClassId;
            
            return new ExpressionCompilationResult(new InstructionBlock(instruction), location, reference: returned);
        }
    }
}