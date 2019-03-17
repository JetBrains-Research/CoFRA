using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PDASimulator.DPDA;
using PDASimulator.Tests.Utils;
using QuickGraph;

namespace PDASimulator.Tests.BlackBox
{
    [TestFixture]
    public static class BracketsInGraph
    {
        private static int ConstructTwoCycles(TestGraph graph, int leftLength, int rightLength)
        {
            var startVertex = leftLength - 1;

            for (int i = startVertex; i >= 0; i--)
            {
                var source = i;
                var target = i > 0 ? i - 1 : startVertex;
                graph.AddVerticesAndEdge(new TaggedEdge<int, string>(source, target, "a"));
            }

            for (int i = startVertex; i < startVertex + rightLength; i++)
            {
                var source = i;
                var target = i < startVertex + rightLength - 1 ? i + 1 : startVertex;
                graph.AddVerticesAndEdge(new TaggedEdge<int, string>(source, target, "b"));
            }

            return startVertex;
        }

        private static bool CheckBracketsCorrectness(string path, IDictionary<string, string> brackets)
        {
            if (path.Length == 0)
            {
                return true;
            }

            var stack = new Stack<string>();

            var splitted = path.Split(' ').Where(token => token.Length > 0);
            foreach (var token in splitted)
            {
                if (brackets.ContainsKey(token))
                {
                    stack.Push(token);
                }
                else
                {
                    if (stack.Count == 0)
                    {
                        return false;
                    }

                    if (token == brackets[stack.Peek()])
                    {
                        stack.Pop();
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return stack.Count == 0;
        }

        private static bool CheckBracketSequencesLength(string path, int leftFactor, int rightFactor)
        {
            var splitted = path.Split(' ').Where(part => part.Length > 0);

            string lastToken = null;
            int currentPartLength = 0;

            bool success = true;
            foreach (var token in splitted)
            {
                if (lastToken != token)
                {
                    if (lastToken == null)
                    {
                        lastToken = token;
                        currentPartLength = 1;
                        continue;
                    }

                    if (lastToken == "a" && currentPartLength % leftFactor > 0)
                    {
                        success = false;
                    }

                    if (lastToken == "b" && currentPartLength % rightFactor > 0)
                    {
                        success = false;
                    }

                    lastToken = token;
                    currentPartLength = 1;
                }
                else
                {
                    currentPartLength++;
                }
            }

            if (lastToken == "a" && currentPartLength % leftFactor > 0)
            {
                success = false;
            }

            return success;
        }

        private static IDictionary<string, string> Brackets(IEnumerable<(string, string)> brackets)
        {
            return brackets.ToDictionary(bracket => bracket.Item1, bracket => bracket.Item2);
        }

        private static int PrepareDataForTwoCyclesNestedBracketsTest(
            int leftLength, 
            int rightLength,
            TestDPDA dpda,
            TestGraph graph)
        {
            dpda.AddTransition((1, -1, "a"), new PushTransition<int, int>(1, 0));
            dpda.AddTransition((1,  0, "a"), new PushTransition<int, int>(1, 0));
            dpda.AddTransition((1,  0, "b"), new PopTransition<int>(2));
            dpda.AddTransition((2,  0, "b"), new PopTransition<int>(2));

            dpda.AddFinal((2, -1));

            var startVertex = ConstructTwoCycles(graph, leftLength, rightLength);

            return startVertex;
        }

        [TestCase(5, 3)]
        [TestCase(4, 3)]
        [TestCase(3, 3)]
        [TestCase(3, 4)]
        [TestCase(3, 5)]
        public static void TwoCyclesNestedBracketsUsingPda(int leftLength, int rightLength)
        {
            TestHelper.InitializePdaProducingSimulation(
                _ => 1, () => -1,
                out var dpda, out var graph, out var simulate);

            var start = PrepareDataForTwoCyclesNestedBracketsTest(leftLength, rightLength, dpda, graph);
            var paths = simulate(start, 2).Distinct();

            foreach (var path in paths)
            {
                Console.WriteLine(path);

                var brackets = Brackets(new[] {("a", "b")});
                Assert.True(CheckBracketsCorrectness(path, brackets));
                Assert.True(CheckBracketSequencesLength(path, leftLength, rightLength));
            }
        }

        private static int PrepareDataForTwoCyclesAllCorrectBracketsTest(
            int leftLength, 
            int rightLength,
            TestDPDA dpda,
            TestGraph graph)
        {
            dpda.AddTransition((1, -1, "a"), new PushTransition<int, int>(1, 0));
            dpda.AddTransition((1,  0, "a"), new PushTransition<int, int>(1, 0));
            dpda.AddTransition((1,  0, "b"), new PopTransition<int>(1));

            dpda.AddFinal((1, -1));

            var startVertex = ConstructTwoCycles(graph, leftLength, rightLength);

            return startVertex;
        }

        [TestCase(5, 3)]
        [TestCase(4, 3)]
        [TestCase(3, 3)]
        [TestCase(3, 4)]
        [TestCase(3, 5)]
        [TestCase(1, 1)]
        public static void TwoCyclesAllCorrectBracketsUsingPda(int leftLength, int rightLength)
        {
            TestHelper.InitializePdaProducingSimulation(
                _ => 1, () => -1,
                out var dpda, out var graph, out var simulate);

            var start = PrepareDataForTwoCyclesAllCorrectBracketsTest(leftLength, rightLength, dpda, graph);
            var paths = simulate(start, 2).Distinct();

            foreach (var path in paths)
            {
                Console.WriteLine(path);

                var brackets = Brackets(new[] {("a", "b")});
                Assert.True(CheckBracketsCorrectness(path, brackets));
                Assert.True(CheckBracketSequencesLength(path, leftLength, rightLength));
            }
        }

        private static int PrepareDataForDifferentBracketsAroundBottleneckTest(
            TestDPDA dpda,
            TestGraph graph)
        {
            dpda.AddTransition((1, new []{-1, 0, 1, 2}, "a"), new PushTransition<int, int>(1, 0));
            dpda.AddTransition((1, new []{-1, 0, 1, 2}, "c"), new PushTransition<int, int>(1, 1));
            dpda.AddTransition((1, new []{-1, 0, 1, 2}, "e"), new PushTransition<int, int>(1, 2));

            dpda.AddTransition((1,  0, "b"), new PopTransition<int>(1));
            dpda.AddTransition((1,  1, "d"), new PopTransition<int>(1));
            dpda.AddTransition((1,  2, "f"), new PopTransition<int>(1));

            dpda.AddFinal((1, -1));

            graph.AddVerticesAndEdge(new TaggedEdge<int, string>(0, 1, "a"));
            graph.AddVerticesAndEdge(new TaggedEdge<int, string>(0, 2, "c"));
            graph.AddVerticesAndEdge(new TaggedEdge<int, string>(1, 3, "e"));
            graph.AddVerticesAndEdge(new TaggedEdge<int, string>(2, 3, "e"));
            graph.AddVerticesAndEdge(new TaggedEdge<int, string>(3, 7, "e"));

            graph.AddVerticesAndEdge(new TaggedEdge<int, string>(7, 8, "f"));
            graph.AddVerticesAndEdge(new TaggedEdge<int, string>(8, 4, "f"));
            graph.AddVerticesAndEdge(new TaggedEdge<int, string>(4, 5, "b"));
            graph.AddVerticesAndEdge(new TaggedEdge<int, string>(4, 6, "d"));

            return 0;
        }

        [Test]
        public static void DifferentBracketsAroundBottleneckUsingPda()
        {
            TestHelper.InitializePdaProducingSimulation(
                _ => 1, () => -1,
                out var dpda, out var graph, out var simulate);

            var start = PrepareDataForDifferentBracketsAroundBottleneckTest(dpda, graph);
            var paths = simulate(start, 0).Distinct();

            foreach (var path in paths)
            {
                var brackets = Brackets(new[] {("a", "b"), ("c", "d"), ("e", "f")});
                CheckBracketsCorrectness(path, brackets);
                Console.WriteLine(path);
            }
        }
    }
}
