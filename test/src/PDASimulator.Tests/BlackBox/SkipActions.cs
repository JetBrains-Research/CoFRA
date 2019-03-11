using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PDASimulator.DPDA;
using PDASimulator.Tests.Utils;
using QuickGraph;

namespace PDASimulator.Tests.BlackBox
{
    [TestFixture]
    class SkipActions
    {
        private int PrepareSkippedBottleneckTest(
            TestDPDA dpda,
            TestGraph graph)
        {
            dpda.AddTransition((1, new []{-1, 0, 1, 2}, "a"), new PushTransition<int, int>(1, 0));
            dpda.AddTransition((1, new []{-1, 0, 1, 2}, "c"), new PushTransition<int, int>(1, 1));
            dpda.AddTransition((1, new []{-1, 0, 1, 2}, "e"), new PushTransition<int, int>(1, 2));

            dpda.AddTransition((1, new []{-1, 0, 1, 2}, "s"), new SkipTransition<int>(1));

            dpda.AddTransition((1,  0, "b"), new PopTransition<int>(1));
            dpda.AddTransition((1,  1, "d"), new PopTransition<int>(1));
            dpda.AddTransition((1,  2, "f"), new PopTransition<int>(1));

            dpda.AddFinal((1, -1));

            graph.AddVerticesAndEdge(new TaggedEdge<int, string>(0, 1, "a"));
            graph.AddVerticesAndEdge(new TaggedEdge<int, string>(0, 2, "c"));
            graph.AddVerticesAndEdge(new TaggedEdge<int, string>(1, 3, "e"));
            graph.AddVerticesAndEdge(new TaggedEdge<int, string>(2, 3, "e"));
            graph.AddVerticesAndEdge(new TaggedEdge<int, string>(3, 7, "s"));

            graph.AddVerticesAndEdge(new TaggedEdge<int, string>(7, 8, "s"));
            graph.AddVerticesAndEdge(new TaggedEdge<int, string>(8, 4, "f"));
            graph.AddVerticesAndEdge(new TaggedEdge<int, string>(4, 5, "b"));
            graph.AddVerticesAndEdge(new TaggedEdge<int, string>(4, 6, "d"));

            return 0;
        }

        [Test]
        public void SkippedBottleneckUsingSppf()
        {
            TestHelper.InitializeSppfProducingSimulation(
                _ => 1, () => -1,
                out var dpda, out var graph, out var simulate);

            var start = PrepareSkippedBottleneckTest(dpda, graph);
            var paths = simulate(start, 2).ToArray();

            foreach (var path in paths)
            {
                Console.WriteLine(path);
            }

            var expected = ImmutableHashSet<string>.Empty.Add("c e s s f d").Add("a e s s f b");
            Assert.True(expected.SetEquals(paths));
        }

        [Test]
        public void SkippedBottleneckUsingPda()
        {
            TestHelper.InitializePdaProducingSimulation(
                _ => 1, () => -1,
                out var dpda, out var graph, out var simulate);

            var start = PrepareSkippedBottleneckTest(dpda, graph);
            var paths = simulate(start, 2);

            var expected = ImmutableHashSet<string>.Empty.Add("c e s s f d").Add("a e s s f b").Add("");
            Assert.True(expected.SetEquals(paths));
        }

        private int PrepareOnSkipBranchMergingTest(
            TestDPDA dpda,
            TestGraph graph)
        {
            dpda.AddTransition((1, new []{-1, 0}, "a"), new PushTransition<int, int>(1, 0));
            dpda.AddTransition((1, new []{-1, 0}, "s"), new SkipTransition<int>(1));
            dpda.AddTransition((1, 0, "b"), new PopTransition<int>(1));

            dpda.AddFinal((1, -1));

            void Edge(int s, int t, string l) => 
                graph.AddVerticesAndEdge(new TaggedEdge<int, string>(s, t, l));

            Edge(0, 1, "s");
            Edge(1, 2, "a");
            Edge(2, 3, "s");
            Edge(3, 4, "s");
            Edge(4, 5, "s");
            Edge(5, 6, "b");

            Edge(1, 7, "a");
            Edge(7, 8, "s");
            Edge(8, 4, "s");

            Edge(0, 9, "a");
            Edge(9, 10, "s");
            Edge(10, 4, "s");

            Edge(0, 11, "a");
            Edge(11, 4, "s");

            Edge(0, 12, "a");
            Edge(12, 5, "s");

            Edge(0, 5, "a");

            return 0;
        }

        [Test]
        public void OnSkipBranchMergingUsingSppf()
        {
            TestHelper.InitializeSppfProducingSimulation(
                _ => 1, () => -1,
                out var dpda, out var graph, out var simulate);

            var start = PrepareOnSkipBranchMergingTest(dpda, graph);
            var paths = simulate(start, 2).ToArray();

            var expected =
                ImmutableList<string>.Empty
                    .Add("a b").Add("a s b").Add("a s s b").Add("a s s s b")
                    .Add("s").Add("s a s s s b").Add("s a s s s b")
                    .Sort();

            foreach (var path in paths)
            {
                Console.WriteLine(path);
            }

            Assert.True(expected.SequenceEqual(paths));
        }

        [Test]
        public void OnSkipBranchMergingUsingPda()
        {
            TestHelper.InitializePdaProducingSimulation(
                _ => 1, () => -1,
                out var dpda, out var graph, out var simulate);

            var start = PrepareOnSkipBranchMergingTest(dpda, graph);
            var paths = simulate(start, 2).ToImmutableList().Sort();

            var expected =
                ImmutableList<string>.Empty
                    .Add("")
                    .Add("a b").Add("a s b").Add("a s s b").Add("a s s s b")
                    .Add("s").Add("s a s s s b").Add("s a s s s b");

            foreach (var path in paths)
            {
                Console.WriteLine(path);
            }

            Assert.True(expected.SequenceEqual(paths));
        }

        private int PrepareSkippedCyclesTest(
            TestDPDA dpda,
            TestGraph graph)
        {
            dpda.AddTransition((1, new []{-1, 0}, "a"), new PushTransition<int, int>(1, 0));
            dpda.AddTransition((1, new []{-1, 0}, "s"), new SkipTransition<int>(1));
            dpda.AddTransition((1, 0, "b"), new PopTransition<int>(1));

            dpda.AddFinal((1, -1));

            void Edge(int s, int t, string l) => 
                graph.AddVerticesAndEdge(new TaggedEdge<int, string>(s, t, l));

            Edge(0, 0, "s");
            Edge(0, 1, "a");
            Edge(1, 2, "s");
            Edge(2, 3, "s");
            Edge(3, 4, "b");
            Edge(1, 0, "s");
            Edge(4, 3, "s");

            return 0;
        }

        [Test]
        public void SkippedCyclesUsingSppf()
        {
            TestHelper.InitializeSppfProducingSimulation(
                _ => 1, () => -1,
                out var dpda, out var graph, out var simulate);

            var start = PrepareSkippedCyclesTest(dpda, graph);
            var paths = simulate(0, 1);

            foreach (var path in paths)
            {
                Console.WriteLine(path);
            }
        }

        [Test]
        public void SkippedCyclesUsingPda()
        {
            TestHelper.InitializePdaProducingSimulation(
                _ => 1, () => -1,
                out var dpda, out var graph, out var simulate);

            var start = PrepareSkippedCyclesTest(dpda, graph);
            var paths = simulate(0, 2);

            foreach (var path in paths)
            {
                Console.WriteLine(path);
            }
        }
    }
}
