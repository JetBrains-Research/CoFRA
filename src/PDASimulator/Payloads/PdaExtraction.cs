using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PDASimulator.DataStructures;
using PDASimulator.DataStructures.GSS;
using PDASimulator.DPDA;
using PDASimulator.DPDA.Simulation;
using PDASimulator.SimulationCommon;

namespace PDASimulator.Payloads
{
    public class ExtractedTransition<TContext, TTransition>
    {
        public readonly TTransition Token;
        public readonly TContext Target;
        public readonly PdaAction Action;

        public ExtractedTransition(
            TTransition token,
            TContext target,
            PdaAction action)
        {
            Token = token;
            Target = target;
            Action = action;
        }

        public override bool Equals(object obj)
        {
            return obj is ExtractedTransition<TContext, TTransition> transition &&
                   EqualityComparer<TTransition>.Default.Equals(Token, transition.Token) &&
                   EqualityComparer<TContext>.Default.Equals(Target, transition.Target) &&
                   Action == transition.Action;
        }

        public override int GetHashCode()
        {
            var hashCode = -998875975;
            hashCode = hashCode * -1521134295 + EqualityComparer<TTransition>.Default.GetHashCode(Token);
            hashCode = hashCode * -1521134295 + EqualityComparer<TContext>.Default.GetHashCode(Target);
            hashCode = hashCode * -1521134295 + Action.GetHashCode();
            return hashCode;
        }
    }

    public class PdaExtractingContext<TGssNode, TTransition> : Context<TGssNode>
    {
        private readonly HashSet<ExtractedTransition<PdaExtractingContext<TGssNode, TTransition>, TTransition>> myTransitions;
    
        public IReadOnlyCollection<ExtractedTransition<PdaExtractingContext<TGssNode, TTransition>, TTransition>> Transitions => myTransitions;

        public PdaExtractingContext(TGssNode stackTop) : base(stackTop)
        {
            myTransitions = new HashSet<ExtractedTransition<PdaExtractingContext<TGssNode, TTransition>, TTransition>>();
        }

        public void AddTransition(ExtractedTransition<PdaExtractingContext<TGssNode, TTransition>, TTransition> transition)
        {
            myTransitions.Add(transition);
        }
    }

    public class PushHistoryRecord<TContext, TTransition>
    {
        public readonly TContext Pusher;
        public readonly TContext Child;
        public readonly TTransition SourceTransition;

        public PushHistoryRecord(TContext pusher, TContext child, TTransition sourceTransition)
        {
            Pusher = pusher;
            Child = child;
            SourceTransition = sourceTransition;
        }

        public override bool Equals(object obj)
        {
            return obj is PushHistoryRecord<TContext, TTransition> history &&
                   EqualityComparer<TContext>.Default.Equals(Pusher, history.Pusher) &&
                   EqualityComparer<TContext>.Default.Equals(Child, history.Child) &&
                   EqualityComparer<TTransition>.Default.Equals(SourceTransition, history.SourceTransition);
        }

        public override int GetHashCode()
        {
            var hashCode = 1054371710;
            hashCode = hashCode * -1521134295 + EqualityComparer<TContext>.Default.GetHashCode(Pusher);
            hashCode = hashCode * -1521134295 + EqualityComparer<TContext>.Default.GetHashCode(Child);
            hashCode = hashCode * -1521134295 + EqualityComparer<TTransition>.Default.GetHashCode(SourceTransition);
            return hashCode;
        }
    }

    public struct PdaExtractingGssData
    {
    }

    public class PdaExtractingContextProcessor<TState, TStackSymbol, TPosition, TTransition> :
        IContextProcessor<TState, TStackSymbol, TPosition, TTransition, 
            PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition>, 
            GssNode<TStackSymbol, PdaExtractingGssData>>
    {
        private readonly Dictionary<GssNode<TStackSymbol, PdaExtractingGssData>, ISet<MyPushHistoryRecord>> myPushHistory;

        public PdaExtractingContextProcessor()
        {
            myPushHistory = new Dictionary<GssNode<TStackSymbol, PdaExtractingGssData>, ISet<MyPushHistoryRecord>>();
        }

        public PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition> HeadToContext(
            Head<TState, TPosition, 
                PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition>, 
                GssNode<TStackSymbol, PdaExtractingGssData>> head)
        {
            return new PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition>(head.StackTop);
        }

        public bool InheritContextsOnPush(PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition> parent, PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition> child, TState nextState,
            TStackSymbol pushed, TTransition sourceTransition, bool memoized)
        {
            AddToPushHistory(child.StackTop, new MyPushHistoryRecord(parent, child, sourceTransition));
            return true;
        }

        public bool InheritContextsOnPop(PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition> parent, PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition> child, TState nextState,
            TTransition sourceTransition, bool memoized)
        {
            foreach (var record in GetOrCreatePushHistory(parent.StackTop))
            {
                record.Pusher.AddTransition(
                    new ExtractedTransition<PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition>, TTransition>(
                        record.SourceTransition, record.Child, PdaAction.Push));
            }

            ClearPushHistory(parent.StackTop);

            parent.AddTransition(
                new ExtractedTransition<PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition>, TTransition>(
                    sourceTransition, child, PdaAction.Pop));

            return true;
        }

        public bool InheritContextsOnSkip(PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition> parent, PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition> child, TState nextState,
            TTransition sourceTransition, bool memoized)
        {
            parent.AddTransition(
                new ExtractedTransition<PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition>, TTransition>(
                    sourceTransition, child, PdaAction.Skip));

            return true;
        }

        public void MergeContexts(
            PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition> source, 
            PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition> target)
        {
            if (ReferenceEquals(source, target))
            {
                return;
            }

            foreach (var transition in target.Transitions)
            {
                source.AddTransition(transition);
            }
        }

        public void MergeGssData(
            GssNode<TStackSymbol, PdaExtractingGssData> source, 
            GssNode<TStackSymbol, PdaExtractingGssData> target)
        {
            MergePushHistoryInto(source, target);
        }

        private ISet<MyPushHistoryRecord> GetOrCreatePushHistory(
            GssNode<TStackSymbol, PdaExtractingGssData> node)
        {
            var exists = myPushHistory.TryGetValue(node, out var history);

            if (!exists)
            {
                history = new HashSet<MyPushHistoryRecord>();
                myPushHistory.Add(node, history);
            }

            return history;
        }

        private void AddToPushHistory(
            GssNode<TStackSymbol, PdaExtractingGssData> node,
            MyPushHistoryRecord record)
        {
            var history = GetOrCreatePushHistory(node);
            history.Add(record);
        }

        private void MergePushHistoryInto(
            GssNode<TStackSymbol, PdaExtractingGssData> source,
            GssNode<TStackSymbol, PdaExtractingGssData> target)
        {
            var sourceHistory = GetOrCreatePushHistory(source);
            var targetHistory = GetOrCreatePushHistory(target);

            targetHistory.UnionWith(sourceHistory);
        }

        private void ClearPushHistory(
            GssNode<TStackSymbol, PdaExtractingGssData> node)
        {
            var history = GetOrCreatePushHistory(node);
            history.Clear();
        }

        private sealed class MyPushHistoryRecord : PushHistoryRecord<PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition>, TTransition>
        {
            public MyPushHistoryRecord(PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition> pusher, PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition> child, TTransition sourceTransition) : base(pusher, child, sourceTransition)
            {
            }
        }
    }
}
