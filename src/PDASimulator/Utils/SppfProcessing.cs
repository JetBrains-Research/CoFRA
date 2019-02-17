using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using PDASimulator.DataStructures.SPPF;
using PDASimulator.Payloads;

namespace PDASimulator.Utils
{
    using Extension = BottomUpSppfConstructingContextExtension;

    public sealed class ReferenceEqualityComparer
        : IEqualityComparer, IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Default
            = new ReferenceEqualityComparer();

        private ReferenceEqualityComparer() { }

        public new bool Equals(object x, object y)
        {
            return x == y;
        }

        public int GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }

    public static class SppfProcessing
    {
        private static ImmutableList<ImmutableList<TTransition>> ExtractPathsInternal<TState, TPosition, TData, TTransition>(
            NonPackedNode<Extension> node,
            ImmutableDictionary<ProducingNode<Extension>, int> visited,
            int maxCyclesExpansion)
        {
            if (node == null)
            {
                return ImmutableList.Create(ImmutableList.Create<TTransition>());
            }

            if (node is ProducingNode<Extension> root)
            {
                int visitsCount = 1;
                if (visited.ContainsKey(root))
                {
                    if (visited[root] > maxCyclesExpansion)
                    {
                        return null;
                    }

                    visitsCount = visited[root] + 1;
                }

                visited = visited.SetItem(root, visitsCount);

                var results =
                    root.Children.SelectMany(
                        packed =>
                        {
                            var left = ExtractPathsInternal<TState, TPosition, TData, TTransition>(
                                packed.Left, visited, maxCyclesExpansion);

                            var right = ExtractPathsInternal<TState, TPosition, TData, TTransition>(
                                packed.Right, visited, maxCyclesExpansion);

                            if (left != null && right != null && left.Any() && right.Any())
                            {
                                return left.SelectMany(
                                    leftAccumulated =>
                                        right.Select(
                                            rightAccumulated =>
                                                leftAccumulated.Concat(rightAccumulated).ToImmutableList()));
                            }

                            return Enumerable.Empty<ImmutableList<TTransition>>();
                        }
                    ).ToImmutableList();

                return results;
            }

            if (node is TerminalNode<Extension, TTransition> terminal)
            {
                return ImmutableList.Create(ImmutableList.Create(terminal.Token));
            }

            return null;
        }

        public static ImmutableList<ImmutableList<TTransition>> ExtractPaths<TState, TPosition, TData, TTransition>(
            CompleteNode<Extension> root,
            int maxCyclesExpansion)
        {
            return ExtractPathsInternal<TState, TPosition, TData, TTransition>(
                root, 
                ImmutableDictionary<ProducingNode<Extension>, int>.Empty,
                maxCyclesExpansion);
        }
    }
}
