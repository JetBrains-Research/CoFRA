using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using NUnit.Framework;
using PDASimulator.DataStructures;
using PDASimulator.DataStructures.GSS;
using PDASimulator.DataStructures.SPPF;
using PDASimulator.DPDA;
using PDASimulator.DPDA.Simulation;
using PDASimulator.Payloads;
using PDASimulator.PDVM.Simulation;
using PDASimulator.Utils;
using QuickGraph;

namespace PDASimulator.Tests.Utils
{
    using TestEdge = TaggedEdge<int, string>;
    
    using TestPdaConstructingContext = PdaExtractingContext<GssNode<int, EmptyGssData>, TaggedEdge<int, string>>;
    using TestPdaConstructingGssNode = GssNode<int, EmptyGssData>;

    using PdaConstructingSimulation =  DPDASimulation<int, int, int, TaggedEdge<int, string>, PdaExtractingContext<GssNode<int, EmptyGssData>, TaggedEdge<int, string>>, EmptyGssData>;

    using PdaConstructingPdvmSimulation = PDVMSimulation<int, int, int, TaggedEdge<int, string>, PdaExtractingContext<GssNode<int, EmptyGssData>, TaggedEdge<int, string>>, EmptyGssData>;

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

    static class TestHelper
    {

        public static void InitializePdaProducingSimulation(
            Func<int, int> startStatesProvider,
            Func<int> startStackSymbolsProvider,
            out TestDPDA dpda,
            out TestGraph graph,
            out Func<int, int, IEnumerable<string>> simulate)
        {
            dpda = new TestDPDA(startStatesProvider, startStackSymbolsProvider);
            graph = new TestGraph();

            var transitionProvider = new TestTransitionProvider(graph);
            var cacheProvider =
                new TestCacheProvider<int, int, int, TestPdaConstructingContext, TestPdaConstructingGssNode>();

            var contextProcessor = new PdaExtractingContextProcessor<int, int, int, TestEdge>();

            var simulation = new PdaConstructingSimulation(
                dpda, contextProcessor, cacheProvider, transitionProvider);

            var finalsInternal = new HashSet<TestPdaConstructingContext>();
            simulation.OnFinalState += head => finalsInternal.Add(head.CurrentContext);

            simulate = (startVertex, maxCyclesExpansion) =>
            {
                var start = simulation.Load(startVertex);
                simulation.Run();

                var paths = new List<string>();
                PdaContextDecoder.ExtractWords(
                    start,
                    context => finalsInternal.Contains(context),
                    maxCyclesExpansion,
                    rawPath =>
                    {
                        var path = string.Join(" ", rawPath.Select(edge => edge.Tag));
                        paths.Add(path);

                        return true;
                    },
                    _ => { });

                using (var writer = new StreamWriter(Path.GetTempPath() + "contexts.dot"))
                {
                    DumpPda(start, writer);
                }

                return paths;
            };
        }

        public static void InitializePdaProducingPdvmSimulation<TPDVM>(
            out TestGraph graph,
            out Func<int, int, IEnumerable<string>> simulate)
            where TPDVM : TestPDVM, new()
        {
            graph = new TestGraph();

            var transitionProvider = new TestTransitionProvider(graph);
            var cacheProvider =
                new TestCacheProvider<int, int, int, TestPdaConstructingContext, TestPdaConstructingGssNode>();

            var contextProcessor = new PdaExtractingContextProcessor<int, int, int, TestEdge>();

            var pdvm = new TPDVM();
            var simulation = new PdaConstructingPdvmSimulation(
                contextProcessor, cacheProvider, transitionProvider,
                () => pdvm);

            simulate = (startVertex, maxCyclesExpansion) =>
            {
                var start = simulation.Load(startVertex, 1, -1);
                simulation.Run();

                var finals = pdvm.Finals;
                var paths = new List<string>();
                PdaContextDecoder.ExtractWords(
                    start,
                    context => finals.Contains(context),
                    maxCyclesExpansion,
                    rawPath =>
                    {
                        var path = string.Join(" ", rawPath.Select(edge => edge.Tag));
                        paths.Add(path);

                        return true;
                    },
                    _ => { });

                foreach (var path in paths)
                {
                    Console.WriteLine(path);
                }

                using (var writer = new StreamWriter(Path.GetTempPath() + "contexts.dot"))
                {
                    DumpPda(start, writer);
                }

                return paths;
            };
        }

        private static void DumpPdaInternal(
            TestPdaConstructingContext current, 
            HashSet<TestPdaConstructingContext> processed,
            Func<TestPdaConstructingContext, int> indexer,
            StreamWriter output)
        {
            if (processed.Contains(current))
            {
                return;
            }

            processed.Add(current);

            var source = indexer(current);

            foreach (var transition in current.Transitions)
            {
                output.WriteLine($"{source} -> {indexer(transition.Target)} [label=\"{transition.Token.Tag}\"]");
                DumpPdaInternal(transition.Target, processed, indexer, output);
            }
        }

        public static void DumpPda(TestPdaConstructingContext root, StreamWriter outputWriter)
        {
            var nextIndex = 0;
            var indexes = new Dictionary<TestPdaConstructingContext, int>(ReferenceEqualityComparer.Default);

            int Indexer(TestPdaConstructingContext context)
            {
                var exists = indexes.TryGetValue(context, out var index);
                if (!exists)
                {
                    index = nextIndex++;
                    indexes[context] = index;
                }

                return index;
            }

            var processed = new HashSet<TestPdaConstructingContext>(ReferenceEqualityComparer.Default);

            outputWriter.WriteLine("digraph Contexts {");

            DumpPdaInternal(root, processed, Indexer, outputWriter);

            outputWriter.WriteLine("}");
        }
    }
}
