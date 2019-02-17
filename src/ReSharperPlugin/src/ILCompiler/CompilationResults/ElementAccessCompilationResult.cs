using System;
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
    public class ElementAccessCompilationResult : ExpressionCompilationResult
    {
        private readonly Reference myArgument;
        
        public ElementAccessCompilationResult(InstructionBlock instructionBlock, Reference argument, Location location = null, 
            Reference reference = null) : base(instructionBlock, location, reference)
        {
            myArgument = argument;
        }

        public override ExpressionCompilationResult TryConvertToGetter(AbstractILCompilerParams @params)
        {
            var invocationTarget = new ClassMethodTarget(new ClassMethodReference(myReference, "get_Item"));
            var passedParams = new Dictionary<ParameterIndex, Reference>();
            if (myArgument != null)
                passedParams.Add(new ParameterIndex(0), myArgument);
            
            var returnedValues = new Dictionary<ParameterIndex, Reference>();
            returnedValues.Add(ParameterIndex.ReturnValueIndex, @params.LocalVariableIndexer.GetNextVariable());
            var statement = new InvocationStatement(GetLocation(),
                invocationTarget,
                passedParams, 
                returnedValues);
              
            var invocationInstruction = new Instruction(statement, @params.GetNewInstructionId());
            var summaryBlock = ElementCompiler.GetInstructionsConnectedSequentially(new List<IInstructionsContainer> {this, invocationInstruction});
            var invocationResultReference = returnedValues[ParameterIndex.ReturnValueIndex];
            return new ExpressionCompilationResult(summaryBlock, GetLocation(), invocationResultReference);
        }

//        public override Reference GetReference()
//        {
//            throw MyParams.GetException("should not be invoked");
//        }

        public override ExpressionCompilationResult TryGetSetter(AbstractILCompilerParams @params, Reference referenceToAssign)
        {
            var invocationTarget = new ClassMethodTarget(new ClassMethodReference(myReference, "set_Item"));
            var returnedValues = new Dictionary<ParameterIndex, Reference>
            {
                {ParameterIndex.ReturnValueIndex, @params.LocalVariableIndexer.GetNextVariable()}
            };
            var passedValues = new Dictionary<ParameterIndex, Reference>{{new ParameterIndex(1), referenceToAssign}};
            if (myArgument != null)
                passedValues.Add(new ParameterIndex(0), myArgument);

            var statement = new InvocationStatement(GetLocation(), invocationTarget, passedValues, returnedValues);  
            var invocationInstruction = new Instruction(statement, @params.GetNewInstructionId());
            var summaryBlock = ElementCompiler.GetInstructionsConnectedSequentially(new List<IInstructionsContainer> {this, invocationInstruction});
            var invocationResultReference = returnedValues[ParameterIndex.ReturnValueIndex];
            return new ExpressionCompilationResult(summaryBlock, GetLocation(), invocationResultReference);
        }
    }
}