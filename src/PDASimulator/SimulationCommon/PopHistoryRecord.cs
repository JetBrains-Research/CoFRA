using System.Collections.Generic;

namespace PDASimulator.SimulationCommon
{
    public class PopHistoryRecord<TState, TTransition, TPosition, TContext>
    {
        public readonly TState NextState;
        public readonly TTransition SourceTransition;
        public readonly TPosition TargetPosition;
        public readonly TContext InheritFrom;

        public PopHistoryRecord(
            TState nextState, 
            TTransition sourceTransition,
            TPosition targetPosition,
            TContext inheritFrom)
        {
            NextState = nextState;
            SourceTransition = sourceTransition;
            InheritFrom = inheritFrom;
            TargetPosition = targetPosition;
        }

        public override bool Equals(object obj)
        {
            return obj is PopHistoryRecord<TState, TTransition, TPosition, TContext> record &&
                   EqualityComparer<TState>.Default.Equals(NextState, record.NextState) &&
                   EqualityComparer<TTransition>.Default.Equals(SourceTransition, record.SourceTransition) &&
                   EqualityComparer<TPosition>.Default.Equals(TargetPosition, record.TargetPosition) &&
                   EqualityComparer<TContext>.Default.Equals(InheritFrom, record.InheritFrom);
        }

        public override int GetHashCode()
        {
            var hashCode = -955327773;
            hashCode = hashCode * -1521134295 + EqualityComparer<TState>.Default.GetHashCode(NextState);
            hashCode = hashCode * -1521134295 + EqualityComparer<TTransition>.Default.GetHashCode(SourceTransition);
            hashCode = hashCode * -1521134295 + EqualityComparer<TPosition>.Default.GetHashCode(TargetPosition);
            hashCode = hashCode * -1521134295 + EqualityComparer<TContext>.Default.GetHashCode(InheritFrom);
            return hashCode;
        }
    }
}