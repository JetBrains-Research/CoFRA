using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.AbstractIL.Common.Types.InvocationTargets;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Resolvers;
using Cofra.AbstractIL.Internal.Statements;
using Cofra.AbstractIL.Internal.Types;
using Cofra.AbstractIL.Internal.Types.Primaries;
using Cofra.AbstractIL.Internal.Types.Secondaries;

namespace Cofra.AbstractIL.Internal.Transducers
{
    public class InvocationsResolvingTransducer<TNode> : AbstractTransducer<TNode>
    {
        private Dictionary<SecondaryEntity, ParameterIndex> AddIntermediateEdgesIfNeeded(
            GraphStructuredProgram<TNode> targetProgram,
            Dictionary<Entity, ParameterIndex> passedParameters,
            TNode currentSource,
            Func<TNode> nodeCreator,
            Func<SecondaryEntity> variableCreator,
            out TNode newSource)
        {
            var processedParameters = passedParameters.ToDictionary(
                pair =>
                {
                    if (pair.Key is PrimaryEntity primary)
                    {
                        var newTarget = nodeCreator();
                        var newVariable = variableCreator();
                        var statement = new ResolvedAssignmentStatement(null, primary, newVariable);
                        var edge = new OperationEdge<TNode>(currentSource, statement, newTarget);

                        targetProgram.AddEdge(edge);
                        currentSource = newTarget;

                        return newVariable;
                    }

                    return (SecondaryEntity) pair.Key;
                },
                pair => pair.Value);

            newSource = currentSource;
            return processedParameters;
        }

        protected override bool Step(
            GraphStructuredProgram<TNode> targetProgram,
            ResolvedMethod<TNode> owningMethod,
            TNode source,
            Statement statement,
            TNode target,
            Func<TNode> nodeCreator)
        {
            SecondaryEntity CreateNewVariable()
            {
                var nextId = owningMethod.Variables.Keys.Max() + 1;
                var variable = new ResolvedLocalVariable(nextId);
                owningMethod.AddLocalVariable(variable);
                return variable;
            }

            if (statement is InvocationStatement invocation)
            {
                var passedParameters = 
                    invocation.PassedParameters.GroupBy(
                            pair => ReferenceResolver.Resolve(targetProgram, owningMethod, pair.Value),
                            pair => pair.Key)
                        .ToDictionary(
                            pair =>
                            {
                                Trace.Assert(pair.Key != null);
                                return pair.Key;
                            },
                            pair =>
                            {
                                var targets = pair.ToList();
                                return targets.Count == 1 ? 
                                    targets[0] : 
                                    new ParameterIndicesSet(
                                        targets.Select(targetIndex => targetIndex.Value));
                            });

                var returnedValues =
                    invocation.ReturnedValues.ToDictionary(
                        pair =>
                        {
                            Trace.Assert(pair.Key != null);
                            return pair.Key;
                        },
                        pair => (SecondaryEntity) ReferenceResolver.Resolve(targetProgram, owningMethod, pair.Value));

                var normalizedPassedParameters = AddIntermediateEdgesIfNeeded(
                    targetProgram, passedParameters, source, nodeCreator, CreateNewVariable, out var newSource);

                ResolvedInvocationStatement<TNode> resolvedInvocationStatement = null;
                if (invocation.InvocationTarget is ClassMethodTarget classMethodTarget)
                {
                    var targetMethodId = targetProgram.GetOrCreateMethodId(classMethodTarget.TargetMethod);
                    var resolvedOwner = ReferenceResolver.Resolve(
                        targetProgram, owningMethod, classMethodTarget.TargetClass);

                    resolvedInvocationStatement =
                        new ResolvedInvocationStatement<TNode>(
                            invocation, normalizedPassedParameters, returnedValues, resolvedOwner, targetMethodId);
                }

                if (invocation.InvocationTarget is LocalVariableTarget localVariableTarget)
                {
                    var resolvedOwner = ReferenceResolver.Resolve(
                        targetProgram, owningMethod, new LocalVariableReference(localVariableTarget.Index));

                    resolvedInvocationStatement =
                        new ResolvedInvocationStatement<TNode>(
                            invocation, normalizedPassedParameters, returnedValues, resolvedOwner, targetProgram.GetOrCreateMethodId("Invoke"));
                }

                if (invocation.InvocationTarget is ClassFieldTarget classFieldTarget)
                {
                    var resolvedOwner = ReferenceResolver.Resolve(
                        targetProgram, owningMethod, new ClassFieldReference(classFieldTarget.TargetClass, classFieldTarget.TargetField));

                    resolvedInvocationStatement =
                        new ResolvedInvocationStatement<TNode>(
                            invocation, normalizedPassedParameters, returnedValues, resolvedOwner, targetProgram.GetOrCreateMethodId("Invoke"));
                }

                if (invocation.InvocationTarget is ClassPropertyTarget classPropertyTarget)
                {
                    var resolvedOwner = ReferenceResolver.Resolve(
                        targetProgram, owningMethod, new ClassPropertyReference(classPropertyTarget.TargetClass, classPropertyTarget.TargetProperty));

                    resolvedInvocationStatement =
                        new ResolvedInvocationStatement<TNode>(
                            invocation, normalizedPassedParameters, returnedValues, resolvedOwner, targetProgram.GetOrCreateMethodId("Invoke"));
                }

                if (invocation.InvocationTarget is LocalFunctionTarget localFunctionTarget)
                {
                    var resolvedOwner = owningMethod;
                    var targetMethodId = targetProgram.GetOrCreateMethodId(localFunctionTarget.TargetFunction.Value);

                    resolvedInvocationStatement =
                        new ResolvedInvocationStatement<TNode>(
                            invocation, normalizedPassedParameters, returnedValues, resolvedOwner, targetMethodId);
                }

                Trace.Assert(resolvedInvocationStatement != null);
                var operation = new Operation<TNode>(resolvedInvocationStatement, target);

                targetProgram.AddOperation(newSource, operation);
                return false;
            }

            return true;
        }
    }
}
