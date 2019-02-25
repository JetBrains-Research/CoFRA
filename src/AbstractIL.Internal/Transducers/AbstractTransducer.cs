using System;
using System.Collections.Generic;
using System.Linq;
using Cofra.AbstractIL.Common;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Statements;
using Cofra.AbstractIL.Internal.Types;
using Cofra.AbstractIL.Internal.Types.Primaries;

namespace Cofra.AbstractIL.Internal.Transducers
{
    public abstract class AbstractTransducer<TNode>
    {
        private AbstractTransducer<TNode> myComposedWith;
        private static readonly NopStatement myNop = new NopStatement();
        private static readonly InternalReturnStatement myReturn = new InternalReturnStatement();

        public AbstractTransducer<TNode> Compose(AbstractTransducer<TNode> another)
        {
            another.myComposedWith = this;
            return another;
        }

        public void Transform(
            INodeBasedProgram<int> sourceMethod,
            GraphStructuredProgram<TNode> targetProgram,
            ResolvedMethod<TNode> targetMethod,
            Func<int, TNode> mapper)
        {
            int counter = -1;
            TNode CreateNewNode()
            {
                var newNode = mapper(counter--);
                targetMethod.AddOwnedNode(newNode);
                return newNode;
            }

            var visited = new HashSet<int>();
            
            var localStarts = sourceMethod.GetStarts();

            var queued = new Stack<(TNode source, int target)>();
            foreach (var localStart in localStarts)
            {
                queued.Push((targetMethod.Start, localStart));
            }

            if (queued.Count == 0)
            {
                targetProgram.AddOperation(targetMethod.Start, new Operation<TNode>(myNop, targetMethod.Final));
            }

            while (queued.Count > 0)
            {
                var (source, rawTarget) = queued.Pop();

                var statement = sourceMethod.StatementAt(rawTarget);
                var target = mapper(rawTarget);

                InternalStep(targetProgram, targetMethod, source, statement, target, CreateNewNode);

                if (visited.Contains(rawTarget))
                {
                    continue;
                }

                if (sourceMethod.IsFinal(rawTarget))
                {
                    targetProgram.AddOperation(target, new Operation<TNode>(myReturn, targetMethod.Final));
                }
                else
                {
                    var transitions = sourceMethod.Transitions(rawTarget).ToList();
                    foreach (var transition in transitions)
                    {
                        queued.Push((target, transition));
                    }
                }

                visited.Add(rawTarget);
            }
        }

        protected abstract bool Step(
            GraphStructuredProgram<TNode> targetProgram,
            ResolvedMethod<TNode> targetMethod,
            TNode source,
            Statement statement,
            TNode target,
            Func<TNode> nodeCreator);

        private bool InternalStep(
            GraphStructuredProgram<TNode> targetProgram,
            ResolvedMethod<TNode> targetMethod,
            TNode source,
            Statement statement,
            TNode target,
            Func<TNode> nodeCreator)
        {
            //TODO: Check steps order correctness
            var toContinue = 
                myComposedWith?.InternalStep(targetProgram, targetMethod, source, statement, target, nodeCreator);

            if (toContinue == null || toContinue.Value)
            {
                return Step(targetProgram, targetMethod, source, statement, target, nodeCreator);
            }

            return false;
        }
    }
}
