using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Statements;
using Cofra.AbstractIL.Internal.Types;
using Cofra.Core.Analyzes.PDVM;
using Cofra.Core.Util;
using PDASimulator.DataStructures;
using PDASimulator.DataStructures.GSS;
using PDASimulator.Payloads;
using PDASimulator.PDVM;
using PDASimulator.PDVM.Simulation;
using PDASimulator.SimulationCommon;

namespace Cofra.Core.Analyzes
{
    using Node = Int32;
    using MyTransition = OperationEdge<int>;
    using MyGssData = Int32;
    using MyGssNode = GssNode<TypePropagation.MyStackData, int>;

    public class TypePropagation : PdvmBasedAnalysis<SecondaryEntity, TypePropagation.MyStackData, TypePropagation.MyContext, MyGssData, bool>
    {
        private static readonly Func<
            GraphStructuredProgram<Node>, 
            PDVM<SecondaryEntity, MyStackData, int, MyTransition, MyContext, MyGssData>> PdvmProvider =
                program => new MyPdvm(program);

        public TypePropagation(
            GraphStructuredProgram<int> program)
            : base(program, new MyContextProcessor(), () => PdvmProvider(program))
        {
        }

        public override bool Analyze(IEnumerable<ResolvedMethod<Node>> starts)
        {
            var initialState = new UnknownEntity();
            var emptyInvocation = new OperationEdge<Node>(0, new NopStatement(), 0);
            var initialStackData = new MyStackData(emptyInvocation, initialState, false);

            var pdvm = (MyPdvm) InternalSimulation.Pdvm;
            pdvm.SetForcedExecutor(InternalSimulation.ForceExecute);

            var contexts = starts.Select(
                start => InternalSimulation.Load(start.Start, initialState, initialStackData)).ToList();

            InternalSimulation.Run();

            return true;
        }

        public void FinishAnalysis()
        {
            InternalSimulation.ExecuteAllQueuedActions();
        }

        public class MyContext : Context<MyGssNode>
        {
            public readonly SecondaryEntity Reference;

            public MyContext(
                GssNode<MyStackData, MyGssData> stackTop,
                SecondaryEntity reference) 
                : base(stackTop)
            {
                Reference = reference;
            }
        }

        public class MyStackData
        {
            public readonly OperationEdge<Node> CallingEdge;
            public readonly SecondaryEntity TrackedEntity;
            public readonly bool Trampoline;

            public MyStackData(MyTransition callingEdge, SecondaryEntity trackedEntity, bool trampoline)
            {
                CallingEdge = callingEdge;
                TrackedEntity = trackedEntity;
                Trampoline = trampoline;
            }

            public override bool Equals(object obj)
            {
                return obj is MyStackData data &&
                       ((!Trampoline && !data.Trampoline) || 
                       CallingEdge.Equals(data.CallingEdge) &&
                       ReferenceEquals(TrackedEntity, data.TrackedEntity));
            }

            public override int GetHashCode()
            {
                var hashCode = -809616716;
                if (!Trampoline)
                {
                    return hashCode;
                }

                hashCode = hashCode * -1521134295 + CallingEdge.GetHashCode();
                hashCode = hashCode * -1521134295 + TrackedEntity.GetHashCode();
                return hashCode;
            }
        }

        public class MyContextProcessor 
            : IContextProcessor<SecondaryEntity, MyStackData, Node, MyTransition, MyContext, MyGssNode>
        {
            public MyContext HeadToContext(Head<SecondaryEntity, int, MyContext, MyGssNode> head)
            {
                return new MyContext(head.StackTop, head.State);
            }

            public bool InheritContextsOnPush(MyContext parent, MyContext child, SecondaryEntity nextState, MyStackData pushed,
                MyTransition sourceTransition, bool memoized)
            {
                var updated = parent.Reference.PropagateUp(child.Reference);

                var newLength = parent.StackTop.UserData + 1;
                if (child.StackTop.UserData < newLength)
                {
                    child.StackTop.UserData = newLength;
                }

                if (newLength > 200)
                {
                    //Console.WriteLine(newLength);
                    return false;
                }

                if (newLength > 200)
                {
                    return updated;
                }

                return true;
            }

            public bool InheritContextsOnPop(MyContext parent, MyContext child, SecondaryEntity nextState, MyTransition sourceTransition,
                bool memoized)
            {
                if (child.Reference is FakeTrampolineEntity fake)
                {
                    var updated = parent.Reference.PropagateDown(fake.OriginalTarget);
                    parent.StackTop.Symbol.TrackedEntity.PropagateForward(fake.OriginalTarget);
                }
                else
                {
                    var updated = parent.Reference.PropagateDown(child.Reference);
                    parent.StackTop.Symbol.TrackedEntity.PropagateForward(child.Reference);
                }

                return true;
            }

            public bool InheritContextsOnSkip(MyContext parent, MyContext child, SecondaryEntity nextState, MyTransition sourceTransition,
                bool memoized)
            {
                if (ReferenceEquals(parent, child))
                {
                    return true;
                }

                var updated = parent.Reference.PropagateForward(child.Reference);
                return true;
            }

            public void MergeContexts(MyContext source, MyContext target)
            {
                throw new NotImplementedException();
            }

            public void MergeGssData(MyGssNode source, MyGssNode target)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class FakeTrampolineEntity : SecondaryEntity
        {
            public readonly SecondaryEntity OriginalTarget;

            public FakeTrampolineEntity(SecondaryEntity originalTarget)
            {
                OriginalTarget = originalTarget;
            }

            public override bool PropagateUp(SecondaryEntity recipient)
            {
                return true;
            }

            public override bool PropagateDown(SecondaryEntity recipient)
            {
                return true;
            }
            
            public override bool PropagateForward(SecondaryEntity recipient)
            {
                return true;
            }

            public override bool Equals(object obj)
            {
                return obj is FakeTrampolineEntity entity &&
                       ReferenceEquals(OriginalTarget, entity.OriginalTarget);
            }

            public override int GetHashCode()
            {
                return 1419405995 + OriginalTarget.GetHashCode();
            }
        }

        private sealed class UnknownEntity : SecondaryEntity
        {
            public override bool PropagateUp(SecondaryEntity recipient)
            {
                return true;
            }

            public override bool PropagateDown(SecondaryEntity recipient)
            {
                return true;
            }
            
            public override bool PropagateForward(SecondaryEntity recipient)
            {
                return true;
            }

            public override int GetHashCode()
            {
                return 293423487;
            }

            public override bool Equals(object obj)
            {
                return ReferenceEquals(this, obj);
            }
        }

        public class MyPdvm : PDVM<SecondaryEntity, MyStackData, Node, MyTransition, MyContext, MyGssData>
        {
            private readonly GraphStructuredProgram<Node> myProcessingProgram;
            private Action<Head<SecondaryEntity, Node, MyContext, MyGssNode>, MyTransition,
                Action<SecondaryEntity, MyGssNode, Node, MyTransition, PrimaryEntity>, PrimaryEntity> myForcedExecutor;

            public MyPdvm(
                GraphStructuredProgram<int> processingProgram)
            {
                myProcessingProgram = processingProgram;
            }

            public void SetForcedExecutor(Action<Head<SecondaryEntity, Node, MyContext, MyGssNode>, MyTransition,
                Action<SecondaryEntity, MyGssNode, Node, MyTransition, PrimaryEntity>, PrimaryEntity> executor)
            {
                myForcedExecutor = executor;
            }

            private IEnumerable<IInvokable<Node>> CollectInstances(SecondaryEntity reference, ResolvedMethodId targetId)
            {
                return reference.CollectedPrimaries.Select(
                    type =>
                    {
                        if (type is ResolvedClassId classId)
                        {
                            var target = myProcessingProgram
                                .FindClassById(classId)
                                .FindMethodInFullHierarchy(targetId);

                            return (IInvokable<Node>) target;
                        }

                        if (type is IInvokable<Node> invocable)
                        {
                            return invocable;
                        }

                        return null;
                    }).Where(target => target != null);
            }

            public override void Action(SecondaryEntity state, MyGssNode stack)
            {
                if (stack.Symbol.Trampoline)
                {
                    if (state is FakeTrampolineEntity fake)
                    {
                        Pop(fake.OriginalTarget, stack.Symbol.CallingEdge.Target);
                    }
                    else
                    {
                        Push(state, new MyStackData(stack.Symbol.CallingEdge, stack.Symbol.TrackedEntity, false));
                    }
                }
            }

            private void InvocationsReExecutor(
                SecondaryEntity state, 
                MyGssNode stack, 
                Node position, 
                MyTransition currentTransition,
                PrimaryEntity newType)
            {
                var invocation = (ResolvedInvocationStatement<Node>) currentTransition.Statement;

                if (newType is IInvokable<Node> invocable)
                {
                    //Logging.Log("dynamic re-invocation");

                    if (state is UnknownEntity)
                    {
                        Push(state, new MyStackData(currentTransition, state, true), invocable.EntryPoint);
                        invocable.MarkAsInvoked();
                    }
                    else
                    {
                        var recipientId = invocation.PassedParameters[state];
                        var exists = invocable.Variables.TryGetValue(recipientId.Value, out var recipient);

                        if (exists)
                        {
                            Push(recipient, new MyStackData(currentTransition, state, true), invocable.EntryPoint);
                        }
                    }
                }

                if (newType is ResolvedClassId classId)
                {
                    var target = myProcessingProgram
                        .FindClassById(classId)
                        .FindMethodInFullHierarchy(invocation.TargetMethodId);

                    if (target == null)
                    {
                        return;
                    }

                    if (state is UnknownEntity)
                    {
                        Push(state, new MyStackData(currentTransition, state, true), target.EntryPoint);
                        target.MarkAsInvoked();
                    }
                    else
                    {
                        var recipientId = invocation.PassedParameters[state];
                        var exists = target.Variables.TryGetValue(recipientId.Value, out var recipient);

                        if (exists)
                        {
                            Push(recipient, new MyStackData(currentTransition, state, true), target.Start);
                        }
                    }
                }
            }

            public override void Step(SecondaryEntity state, MyGssNode stack, Node position, MyTransition currentTransition)
            {
                if (stack.Symbol.Trampoline)
                {
                    return;
                }

                var statement = currentTransition.Statement;
                switch (statement.Type)
                {
                    case StatementType.Internal:
                        var internalStatement = (InternalStatement) statement;

                        ParameterIndex recipientId;
                        bool exists;
                        switch (internalStatement.InternalType)
                        {
                            case InternalStatementType.ResolvedInvocation:
                                var invocation = (ResolvedInvocationStatement<Node>) internalStatement;
                                exists = invocation.PassedParameters.TryGetValue(state, out recipientId);
                                var scanning = state is UnknownEntity;

                                Skip(state);

                                if (exists || scanning)
                                {
                                    if (invocation.TargetEntity is SecondaryEntity variable)
                                    {
                                        var instances = CollectInstances(
                                            variable,
                                            invocation.TargetMethodId);

                                        foreach (var instance in instances.ToList())
                                        {
                                            //Logging.Log("dynamic invocation");
                                            if (exists)
                                            {
                                                instance.Variables.TryGetValue(recipientId.Value, out var recipient);
                                                if (recipient != null)
                                                {
                                                    Push(recipient, new MyStackData(currentTransition, state, true),
                                                        instance.EntryPoint);
                                                    //Logging.Log("recipient is not null");
                                                }
                                                else
                                                {
                                                    //Logging.Log($"recipient is null: {myProcessingProgram.GetMethodByStartPoint(instance.EntryPoint)}, {recipientId.Value}");
                                                }
                                            }

                                            if (scanning)
                                            {
                                                Push(state, new MyStackData(currentTransition, state, true), instance.EntryPoint);
                                                instance.MarkAsInvoked();
                                            }
                                        }

                                        var head = ExtractHead();
                                        variable.Subscriptions.OnNewPrimaryAdded +=
                                            newType =>
                                            {
                                                myForcedExecutor(head, currentTransition,
                                                    InvocationsReExecutor, newType);
                                            };
                                    }

                                    if (invocation.TargetEntity is ResolvedClassId classId)
                                    {
                                        var clazz = myProcessingProgram.FindClassById(classId);
                                        var method = clazz.FindMethodInFullHierarchy(invocation.TargetMethodId);

                                        if (method != null)
                                        {
                                            if (exists)
                                            {
                                                method.Variables.TryGetValue(recipientId.Value, out var recipient);
                                                if (recipient != null)
                                                {
                                                    Push(recipient, new MyStackData(currentTransition, state, true),
                                                        method.Start);
                                                    //Logging.Log("recipient is not null");
                                                }
                                                else
                                                {
                                                    //Logging.Log($"recipient is null: {myProcessingProgram.GetMethodByStartPoint(method.EntryPoint)}, {recipientId.Value}");
                                                }
                                            }

                                            if (scanning)
                                            {
                                                Push(state, new MyStackData(currentTransition, state, true), method.EntryPoint);
                                                method.MarkAsInvoked();
                                            }
                                        }
                                    }
                                }

                                break;
                            case InternalStatementType.Return:
                                var stackTop = stack.Symbol;
                                if (stackTop.CallingEdge.Statement is NopStatement)
                                {
                                    break;
                                }

                                var callingStatement = (ResolvedInvocationStatement<Node>) stackTop.CallingEdge.Statement;

                                if (state is ResolvedLocalVariable localVariable)
                                {
                                    var id = new ParameterIndex(localVariable.LocalId);
                                    exists = callingStatement.ReturnedValues.TryGetValue(id, out var recipient);
                                    if (exists)
                                    {
                                        Pop(new FakeTrampolineEntity(recipient));
                                    }
                                }

                                break;
                            case InternalStatementType.ResolvedAssignment:
                                var assignment = (ResolvedAssignmentStatement) currentTransition.Statement;

                                Skip(state);

                                //Trace.Assert(assignment.Target != null);
                                if (assignment.Target == null)
                                {
                                    break;
                                }

                                if (state is UnknownEntity)
                                {
                                    if (assignment.Source is PrimaryEntity primary)
                                    {
                                        assignment.Target.AddPrimary(primary);
                                        Accept(assignment.Target);
                                    }
                                    else
                                    {
                                        ((SecondaryEntity) assignment.Source).PropagateForward(assignment.Target);
                                        if (assignment.Target is ResolvedLocalVariable)
                                        {
                                            Accept(assignment.Target);
                                        }
                                    }
                                }

                                /*
                                if ((assignment.Source is ResolvedObjectField ||
                                     assignment.Source is ResolvedClassField ||
                                     assignment.Source is ResolvedObjectMethodReference<Node>) &&
                                    state is UnknownEntity)
                                {
                                    var asSecondary = (SecondaryEntity) assignment.Source;
                                    asSecondary.PropagateForward(assignment.Target);
                                    Accept(assignment.Target);
                                }
                                */

                                if (ReferenceEquals(assignment.Source, state))
                                {
                                    Accept(assignment.Target);
                                }

                                break;
                        }
                        break;
                    default:
                        Skip(state);
                        break;
                }
            }

            protected override void OnFinish(Head<SecondaryEntity, int, MyContext, MyGssNode> head, MyTransition sourceTransition)
            {
            }
        }
    }
}
