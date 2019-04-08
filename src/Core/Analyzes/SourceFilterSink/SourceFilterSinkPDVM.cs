using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Statements;
using Cofra.AbstractIL.Internal.Types;
using Cofra.AbstractIL.Internal.Types.Primaries;
using Cofra.AbstractIL.Internal.Types.Secondaries;
using PDASimulator.DataStructures.GSS;
using PDASimulator.Payloads;
using PDASimulator.PDVM;
using PDASimulator.SimulationCommon;

namespace Cofra.Core.Analyzes.SourceFilterSink
{
    using Node = Int32;
    using State = SecondaryEntity;
    using StackSymbol = OperationEdge<Int32>;
    using Transition = OperationEdge<Int32>;
    using GssNode = GssNode<OperationEdge<Int32>, EmptyGssData>;
    using Context = PdaExtractingContext<GssNode<OperationEdge<Int32>, EmptyGssData>, OperationEdge<Int32>>;

    public sealed class SourceFilterSinkPDVM : 
        PDVM<State, StackSymbol, Node, Transition, Context, EmptyGssData>
    {
        private readonly GraphStructuredProgram<Node> myProcessingProgram;

        private readonly int mySinkAttributeId;
        private readonly int myFilterAttributeId;

        public event Action<Head<State, Node, Context, GssNode>, Transition> OnFinishEvent;

        public SourceFilterSinkPDVM(GraphStructuredProgram<int> processingProgram)
        {
            myProcessingProgram = processingProgram;

            mySinkAttributeId = myProcessingProgram.GetOrCreateAttribute(Constants.SinkAttribute);
            myFilterAttributeId = myProcessingProgram.GetOrCreateAttribute(Constants.FilterAttribute);
        }

        public override void Action(
            State state, 
            GssNode<StackSymbol, EmptyGssData> stack)
        {
            if (state is FinalState)
            {
                if (!stack.Pop().Any())
                {
                    Finish();
                }
                else
                {
                    Pop(state);
                }
            }
        }

        public override void Step(
            State state, 
            GssNode<StackSymbol, EmptyGssData> stack, 
            Node position, 
            Transition currentTransition)
        {
            var commonStatement = currentTransition.Statement;
            if (commonStatement.Type != StatementType.Internal)
            {
                return;
            }

            if ((state is FinalState))
                return;

            Skip(state);

            var statement = (InternalStatement) commonStatement;

            switch (statement.InternalType)
            {
                case InternalStatementType.ResolvedInvocation:
                    ProcessInvocation(state, (ResolvedInvocationStatement<Node>) statement, currentTransition);
                    break;

                case InternalStatementType.Return:
                    ProcessReturn(state, (InternalReturnStatement) statement, stack);
                    break;

                case InternalStatementType.ResolvedAssignment:
                    ProcessAssignment(state, (ResolvedAssignmentStatement) statement);
                    break;
            }
        }

        protected override void OnFinish(
            Head<State, int, Context, GssNode<StackSymbol, EmptyGssData>> head, 
            Transition sourceTransition)
        {
            OnFinishEvent?.Invoke(head, sourceTransition);
        }
        
        private IEnumerable<IInvokable<Node>> CollectInstances(Entity reference, ResolvedMethodId targetId)
        {
            if (reference is ResolvedClassId staticClass)
            {
                IEnumerable<IInvokable<Node>> Iterator(ResolvedMethod<Node> method)
                {
                    if (method != null)
                    {
                        yield return method;
                    }
                }

                var resolvedMethod = myProcessingProgram.FindClassById(staticClass).FindMethodInFullHierarchy(targetId);
                return Iterator(resolvedMethod);
            }

            if (reference is SecondaryEntity secondary)
            {
                return secondary.CollectedPrimaries.Select(
                    type =>
                    {
                        if (type is ResolvedClassId classId)
                        {
                            var target = myProcessingProgram
                                .FindClassById(classId)
                                .FindMethodInFullHierarchy(targetId);

                            return (IInvokable<Node>) target;
                        }

                        if (type is IInvokable<Node> invokable)
                        {
                            return invokable;
                        }

                        return null;
                    }).Where(target => target != null);
            }

            return Enumerable.Empty<IInvokable<Node>>();
        }

        private void ProcessAssignment(State state, ResolvedAssignmentStatement assignment)
        {
            if (!(state is ResolvedLocalVariable))
            {
                if (Utils.IsTaintedSource(assignment.Source))
                {
                    var nextState = Utils.FindClosestLocalOwner(assignment.Target);
                    Accept(nextState);
                }

                return;
            }

            var closestTarget = Utils.FindClosestLocalOwner(assignment.Target);
            if (closestTarget == null)
            {
                return;
            }

            var closestSource = Utils.FindClosestLocalOwner(assignment.Source as SecondaryEntity);
            if (closestSource == null)
            {
                return;
            }

            if (closestSource.Equals(state))
            {
                Accept(closestTarget);
            }
        }

        private void ProcessInvocation(
            State state,
            ResolvedInvocationStatement<Node> invocation,
            Transition currentTransition)
        {
            var targets = CollectInstances(invocation.TargetEntity, invocation.TargetMethodId);
            var passedParameters = invocation.PassedParameters.ToDictionary(
                pair => Utils.FindClosestLocalOwner(pair.Key), pair => pair.Value);

            if (state is ResolvedLocalVariable localState)
            {
                if (!passedParameters.ContainsKey(localState))
                {
                    return;
                }

                var parameterIndex = passedParameters[localState].Value;

                foreach (var target in targets)
                {
                    var isSink = false;
                    var isFilter = false;
                    if (target is ResolvedMethod<Node> method)
                    {
                        isSink = method.HasAttribute(mySinkAttributeId);
                        isFilter = method.HasAttribute(myFilterAttributeId);
                    }

                    if (isSink)
                    {
                        Accept(new FinalState());
                    }
                    else
                    {
                        if (!isFilter)
                        {
                            var nextState = target.Variables[parameterIndex];
                            Push(nextState, currentTransition, target.EntryPoint);
                        }
                    }
                }
            }
            else
            {
                foreach (var target in targets)
                {
                    Push(state, currentTransition, target.EntryPoint);
                }
            }
        }

        private void ProcessReturn(
            State state,
            InternalReturnStatement returnStatement,
            GssNode<StackSymbol, EmptyGssData> stack)
        {
            if (!(state is ResolvedLocalVariable localState))
            {
                return;
            }

            if (!(stack.Symbol.Statement is ResolvedInvocationStatement<Node> callingStatement))
            {
                return;
            }

            var returnedValues = callingStatement.ReturnedValues;
            var currentIndex = new ParameterIndex(localState.LocalId);

            var exists = returnedValues.TryGetValue(currentIndex, out var nextState);

            if (exists)
            {
                Pop(nextState, stack.Symbol.Target);
            }
        }

        public class InitialDummyState : State
        {
            public override bool Equals(object another)
            {
                return another is InitialDummyState;
            }

            public override int GetHashCode()
            {
                return 7875832;
            }
        }

        public class FinalState : State
        {
            public override bool Equals(object another)
            {
                return another is FinalState;
            }

            public override int GetHashCode()
            {
                return 787583224;
            }
        }
    }
}
