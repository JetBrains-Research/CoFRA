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
    /*
    [TestFixture]
    class Epsilonization 
    {
        [Test]
        public void EpsilonizedBottleneck()
        {
            TestHelper.InitializeSimulation(
                _ => 1, 
                () => -1, 
                out var simulation, 
                out var dpda, 
                out var graph,
                out var getSppfRoots);

            dpda.AddTransition((1, new []{-1, 0, 1, 2}, "a"), new PdaTransition<int, int>(PdaAction.Push, 0, 1));
            dpda.AddTransition((1, new []{-1, 0, 1, 2}, "c"), new PdaTransition<int, int>(PdaAction.Push, 1, 1));
            dpda.AddTransition((1, new []{-1, 0, 1, 2}, "e"), new PdaTransition<int, int>(PdaAction.Push, 2, 1));

            dpda.AddTransition((1, new []{-1, 0, 1, 2}, "s"), new PdaTransition<int, int>(PdaAction.Epsilonize, -1, 1));

            dpda.AddTransition((1,  0, "b"), new PdaTransition<int, int>(PdaAction.Pop, -1, 1));
            dpda.AddTransition((1,  1, "d"), new PdaTransition<int, int>(PdaAction.Pop, -1, 1));
            dpda.AddTransition((1,  2, "f"), new PdaTransition<int, int>(PdaAction.Pop, -1, 1));

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

            var root = simulation.Load(0);

            simulation.Run();

            var sppfRoots = getSppfRoots(root).ToArray();
            var paths =
                sppfRoots.SelectMany(sppfRoot =>
                    TestHelper
                        .ExtractPaths(sppfRoot, 2)
                        .Select(path => string.Join(' ', path))
                        .Distinct());

            foreach (var path in paths)
            {
                Console.WriteLine(path);
            }

            using (var outputWriter = new StreamWriter(@"C:\hackathon\contexts.dot"))
            {
                TestHelper.DumpSppfGraph(sppfRoots, outputWriter);
            }
        }

        [Test]
        public void OnEpsilonizationBranchMerging()
        {
            TestHelper.InitializeSimulation(
                _ => 1, 
                () => -1, 
                out var simulation, 
                out var dpda, 
                out var graph,
                out var getSppfRoots);

            dpda.AddTransition((1, new []{-1, 0}, "a"), new PdaTransition<int, int>(PdaAction.Push, 0, 1));
            dpda.AddTransition((1, new []{-1, 0}, "s"), new PdaTransition<int, int>(PdaAction.Epsilonize, -1, 1));
            dpda.AddTransition((1, 0, "b"), new PdaTransition<int, int>(PdaAction.Pop, -1, 1));

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

            var root = simulation.Load(0);

            simulation.Run();

            var sppfRoots = getSppfRoots(root).ToArray();
            var paths =
                sppfRoots.SelectMany(sppfRoot =>
                        TestHelper
                            .ExtractPaths(sppfRoot, 2)
                            .Select(path => string.Join(' ', path)))
                    .ToImmutableList()
                    .Sort(Comparer<string>.Default);

            var expected =
                ImmutableList<string>.Empty
                    .Add("a b").Add("a s b").Add("a s s b").Add("a s s s b")
                    .Add("s").Add("s a s s s b").Add("s a s s s b");

            foreach (var path in paths)
            {
                Console.WriteLine(path);
            }

            //Assert.True(expected.SequenceEqual(paths));

            using (var outputWriter = new StreamWriter(@"C:\hackathon\contexts.dot"))
            {
                TestHelper.DumpSppfGraph(sppfRoots, outputWriter);
            }
        }

        [Test]
        public void EpsilonizedCycles()
        {
            TestHelper.InitializeSimulation(
                _ => 1, 
                () => -1, 
                out var simulation,
                out var dpda,
                out var graph,
                out var getSppfRoots);

            dpda.AddTransition((1, new []{-1, 0}, "a"), new PdaTransition<int, int>(PdaAction.Push, 0, 1));
            dpda.AddTransition((1, new []{-1, 0}, "s"), new PdaTransition<int, int>(PdaAction.Epsilonize, -1, 1));
            dpda.AddTransition((1, 0, "b"), new PdaTransition<int, int>(PdaAction.Pop, -1, 1));

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

            var root = simulation.Load(0);

            simulation.Run();

            var sppfRoots = getSppfRoots(root).ToArray();
            var paths =
                sppfRoots.SelectMany(sppfRoot =>
                        TestHelper
                            .ExtractPaths(sppfRoot, 2)
                            .Select(path => string.Join(' ', path)))
                    .Distinct()
                    .ToImmutableList()
                    .Sort(Comparer<string>.Default);

            foreach (var path in paths)
            {
                Console.WriteLine(path);
            }

            using (var outputWriter = new StreamWriter(@"C:\hackathon\contexts.dot"))
            {
                TestHelper.DumpSppfGraph(sppfRoots, outputWriter);
            }
        }
    }
    */
}
