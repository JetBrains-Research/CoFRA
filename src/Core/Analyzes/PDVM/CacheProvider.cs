using System;
using System.Collections.Generic;
using System.Text;
using PDASimulator.SimulationCommon;

namespace Cofra.Core.Analyzes.PDVM
{
    using Node = Int32;

    public class CacheProvider<TState, TStackSymbol, TContext, TGssNode> 
        : ICacheProvider<TState, TStackSymbol, Node, TContext, TGssNode>
    {
        private readonly Dictionary<VisitHistoryRecord, TContext> myVisitsInfo;
        private readonly Dictionary<VisitOnPopHistoryRecord, TContext> myOnPopVisitsInfo;

        public CacheProvider()
        {
            myVisitsInfo = new Dictionary<VisitHistoryRecord, TContext>();
            myOnPopVisitsInfo = new Dictionary<VisitOnPopHistoryRecord, TContext>();
        }

        public bool Visited(int position, TState inState, TStackSymbol withData, out TContext byContext)
        {
            return myVisitsInfo.TryGetValue(new VisitHistoryRecord(position, inState, withData), out byContext);
        }

        public void Visit(int position, TState state, TStackSymbol data, TContext context)
        {
            myVisitsInfo.Add(new VisitHistoryRecord(position, state, data), context);
        }

        public bool VisitedOnPop(int position, TState targetState, TGssNode targetTop, out TContext byContext)
        {
            return myOnPopVisitsInfo.TryGetValue(new VisitOnPopHistoryRecord(position, targetState, targetTop), out byContext);
        }

        public void VisitOnPop(int position, TState targetState, TGssNode targetTop, TContext byContext)
        {
            myOnPopVisitsInfo.Add(new VisitOnPopHistoryRecord(position, targetState, targetTop), byContext);
        }

        private struct VisitOnPopHistoryRecord
        {
            public readonly Node Position;
            public readonly TState State;
            public readonly TGssNode Stack;

            public VisitOnPopHistoryRecord(int position, TState state, TGssNode stack)
            {
                Position = position;
                State = state;
                Stack = stack;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is VisitOnPopHistoryRecord))
                {
                    return false;
                }

                var record = (VisitOnPopHistoryRecord)obj;
                return Position == record.Position &&
                       State.Equals(record.State) &&
                       ReferenceEquals(Stack, record.Stack);
            }

            public override int GetHashCode()
            {
                var hashCode = 691247303;
                hashCode = hashCode * -1521134295 + Position.GetHashCode();
                hashCode = hashCode * -1521134295 + State.GetHashCode();
                hashCode = hashCode * -1521134295 + Stack.GetHashCode();
                return hashCode;
            }
        }

        private struct VisitHistoryRecord
        {
            public readonly Node Position;
            public readonly TState State;
            public readonly TStackSymbol StackSymbol;

            public VisitHistoryRecord(Node position, TState state, TStackSymbol stackSymbol)
            {
                Position = position;
                State = state;
                StackSymbol = stackSymbol;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is VisitHistoryRecord))
                {
                    return false;
                }

                var record = (VisitHistoryRecord) obj;
                return Position == record.Position &&
                       State.Equals(record.State) &&
                       StackSymbol.Equals(record.StackSymbol);
            }

            public override int GetHashCode()
            {
                var hashCode = 691247303;
                hashCode = hashCode * -1521134295 + Position.GetHashCode();
                hashCode = hashCode * -1521134295 + State.GetHashCode();
                hashCode = hashCode * -1521134295 + StackSymbol.GetHashCode();
                return hashCode;
            }
        }
    }
}
