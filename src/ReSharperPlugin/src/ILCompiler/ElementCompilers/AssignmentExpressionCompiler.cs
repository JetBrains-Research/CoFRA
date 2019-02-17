using System;
using System.Collections.Generic;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Common.Types;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resources;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class AssignmentExpressionCompiler : ExpressionCompiler
    {
        private IAssignmentExpression myAssignmentExpression;        
        public AssignmentExpressionCompiler(IAssignmentExpression assignmentExpression, AbstractILCompilerParams @params) : base(@params)
        {
            myAssignmentExpression = assignmentExpression;
        }

        public override ICompilationResult GetResult()
        {
            var location = GetLocation(myAssignmentExpression);
            
            var sourceExpression = myAssignmentExpression.Source;
            if (sourceExpression == null) return new ElementCompilationResult();;
            var assignmentSource = MyChildToResult[sourceExpression];
            if (!(assignmentSource is IExpressionCompilationResult source))
                throw MyParams.CreateException($"source of assignment is not an expression");
            // need getter
            source = source.TryConvertToGetter(MyParams);
            var sourceOfAssignment = source.GetReference();
            
            var referenceToTarget = myAssignmentExpression.Dest;
            if (referenceToTarget == null) return new ElementCompilationResult();;
            var assignmentTarget = MyChildToResult[referenceToTarget];
            if (!(assignmentTarget is IExpressionCompilationResult target))
                throw MyParams.CreateException($"target of assignment is not an expression");
            
            // !! setter
            var targetReference = target.GetReference();
            
            if (sourceOfAssignment == null || targetReference == null)
            {
                return new ExpressionCompilationResult();
            }

            var instructions = new List<IInstructionsContainer>{source};
            // divide assignment on 2 parts: a = b ->> newVar = b; a = newVar
            if (sourceOfAssignment is ClassMemberReference)
            {
                var extraVariable = MyParams.LocalVariableIndexer.GetNextVariable();
                var extraAssignment = new AssignmentStatement(location, sourceOfAssignment, extraVariable);
                var extraInstruction = new Instruction(extraAssignment, MyParams.GetNewInstructionId());
                instructions.Add(extraInstruction);
                sourceOfAssignment = extraVariable;
            }
            
            var withSetter = target.TryGetSetter(MyParams, sourceOfAssignment);
            if (withSetter != null)
            {
                instructions.Add(withSetter);
                return new ExpressionCompilationResult(GetInstructionsConnectedSequentially(instructions), location, withSetter.GetReference());
            }

            instructions.Add(target);
            
            //!!! workaround for "this = x;"
            if (!(targetReference is ClassReference))
            {
                if (!(targetReference is ClassFieldReference || targetReference is LocalVariableReference))
                    throw MyParams.CreateException($"assignment to {target.GetType()}"); 
                var mainAssignmentStatement = new AssignmentStatement(location, sourceOfAssignment, targetReference);
                var instruction = new Instruction(mainAssignmentStatement, MyParams.GetNewInstructionId());
                instructions.Add(instruction);
            }

            return new ExpressionCompilationResult(GetInstructionsConnectedSequentially(instructions), location, targetReference);
        }
    }
}