using System.Collections.Generic;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.AnalysisSpecific;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.AbstractIL.Common.Types.InvocationTargets;
using Cofra.ReSharperPlugin.ILCompiler.ElementCompilers;
using JetBrains.Util;

namespace Cofra.ReSharperPlugin.ILCompiler.CompilationResults
{
    public class ExpressionCompilationResult : ElementCompilationResult, IExpressionCompilationResult
    {
        protected readonly Reference myReference;
        protected readonly Location myLocation;

        public ExpressionCompilationResult()
        {
            myReference = null;
            myLocation = null;
        }

        public ExpressionCompilationResult(InstructionBlock instructionBlock, Location location, 
            Reference reference = null) : base(instructionBlock)
        {
            myReference = reference;
            myLocation = location;
        }

        public virtual ExpressionCompilationResult TryConvertToGetter(AbstractILCompilerParams @params)
        {
            if (!(myReference is ClassPropertyReference propertyReference)) return this;
            
            var invocationTarget = new ClassMethodTarget(new ClassMethodReference(propertyReference.Owner, $"get_{propertyReference.Name}"));
            var returnedValues = new Dictionary<ParameterIndex, Reference>();
            returnedValues.Add(ParameterIndex.ReturnValueIndex, @params.LocalVariableIndexer.GetNextVariable());
            var statement = new InvocationStatement(GetLocation(),
                invocationTarget,
                new Dictionary<ParameterIndex, Reference>(), 
                returnedValues);
              
            var invocationInstruction = new Instruction(statement, @params.GetNewInstructionId());
            var summaryBlock = ElementCompiler.GetInstructionsConnectedSequentially(new List<IInstructionsContainer> {this, invocationInstruction});
            var invocationResultReference = returnedValues[ParameterIndex.ReturnValueIndex];
            return new ExpressionCompilationResult(summaryBlock, GetLocation(), invocationResultReference);
        }

        public virtual Reference GetReference()
        {
            return myReference;
        }

        public virtual ExpressionCompilationResult TryGetSetter(AbstractILCompilerParams @params, Reference referenceToAssign)
        {
            if (!(myReference is ClassPropertyReference propertyReference)) return null;
            
            var invocationTarget = new ClassMethodTarget(new ClassMethodReference(propertyReference.Owner, $"set_{propertyReference.Name}"));
            var returnedValues = new Dictionary<ParameterIndex, Reference>();
            returnedValues.Add(ParameterIndex.ReturnValueIndex, @params.LocalVariableIndexer.GetNextVariable());
            var passedValues = new Dictionary<ParameterIndex, Reference> {{new ParameterIndex(0), referenceToAssign}};

            var statement = new InvocationStatement(GetLocation(), invocationTarget, passedValues, returnedValues);  
            var invocationInstruction = new Instruction(statement, @params.GetNewInstructionId());
            var summaryBlock = ElementCompiler.GetInstructionsConnectedSequentially(new List<IInstructionsContainer> {this, invocationInstruction});
            var invocationResultReference = returnedValues[ParameterIndex.ReturnValueIndex];
            return new ExpressionCompilationResult(summaryBlock, GetLocation(), invocationResultReference);
        }
        
        protected Location GetLocation()
        {
            return myLocation;
        }
    }
}