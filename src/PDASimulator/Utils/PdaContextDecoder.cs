using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using PDASimulator.DataStructures;
using PDASimulator.DataStructures.GSS;
using PDASimulator.Payloads;
using PDASimulator.DPDA;

namespace PDASimulator.Utils
{
    using PdaExtractingGssData = EmptyGssData;

    public static class PdaContextDecoder
    {
        private static bool ExtractWordsInternal<TStackSymbol, TTransition>(
            PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition> root,
            Func<PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition>, bool> isFinal,
            ImmutableDictionary<PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition>, int> visited,
            ImmutableStack<TStackSymbol> stack,
            ImmutableQueue<TTransition> accumulator,
            Func<IEnumerable<TTransition>, bool> onExtracted,
            int maxCyclesExpansion,
            Action<IEnumerable<TTransition>> peek) 
        {
            if (visited.ContainsKey(root) && visited[root] >= maxCyclesExpansion + 1)
            {
                return true;
            }

            if (isFinal(root))
            {
                if (stack.IsEmpty && !root.StackTop.Pop().Any() ||
                    root.StackTop.Symbol.Equals(stack.Peek()))
                {
                    var shouldContinue = onExtracted(accumulator);
                    if (!shouldContinue)
                    {
                        return false;
                    }
                }
            }

            if (visited.ContainsKey(root))
            {
                var count = visited[root] + 1;
                visited = visited.SetItem(root, count);
            }
            else
            {
                visited = visited.SetItem(root, 1);
            }

            peek(accumulator);

            foreach (var transition in root.Transitions)
            {
                var shouldContinue = true;
                switch (transition.Action)
                {
                    case PdaAction.Push:
                        shouldContinue =
                            ExtractWordsInternal(
                                transition.Target,
                                isFinal,
                                visited,
                                stack.Push(transition.Target.StackTop.Symbol),
                                accumulator.Enqueue(transition.Token),
                                onExtracted,
                                maxCyclesExpansion,
                                peek);

                        break;
                    case PdaAction.Skip:
                        shouldContinue = 
                            ExtractWordsInternal(
                                transition.Target,
                                isFinal,
                                visited,
                                stack,
                                accumulator.Enqueue(transition.Token),
                                onExtracted,
                                maxCyclesExpansion,
                                peek);
                        break;
                    case PdaAction.Pop:
                        if (stack.IsEmpty)
                        {
                            break;
                        }

                        var gss = transition.Target.StackTop;
                        var popped = stack.Pop();

                        if (popped.IsEmpty)
                        {
                            if (gss.Pop().Any())
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (!popped.Peek().Equals(gss.Symbol))
                            {
                                break;
                            }
                        }

                        shouldContinue = 
                            ExtractWordsInternal(
                                transition.Target,
                                isFinal,
                                visited,
                                popped,
                                accumulator.Enqueue(transition.Token),
                                onExtracted,
                                maxCyclesExpansion,
                                peek);
                        break;
                }

                if (!shouldContinue)
                {
                    return false;
                }
            }

            return true;
        }

        public static void ExtractWords<TStackSymbol, TTransition>(
            PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition> root, 
            Func<PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition>, bool> isFinal, 
            int maxCyclesExpansion,
            Func<IEnumerable<TTransition>, bool> onExtracted,
            Action<IEnumerable<TTransition>> peek)
        {
            ExtractWordsInternal(
                root, 
                isFinal,
                ImmutableDictionary<PdaExtractingContext<GssNode<TStackSymbol, PdaExtractingGssData>, TTransition>, int>.Empty,
                ImmutableStack<TStackSymbol>.Empty,
                ImmutableQueue<TTransition>.Empty,
                onExtracted,
                maxCyclesExpansion,
                peek);
        }
    }
}
