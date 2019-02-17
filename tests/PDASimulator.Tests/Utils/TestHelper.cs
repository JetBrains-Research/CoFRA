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
    using Extension = BottomUpSppfConstructingContextExtension;
    using TestEdge = TaggedEdge<int, string>;

    using TestSppfConstructingContext = BottomUpSppfConstructingContext<GssNode<int, BottomUpSppfConstructingGssData>>;
    using TestSppfConstructingGssNode = GssNode<int, BottomUpSppfConstructingGssData>;
    
    using TestPdaConstructingContext = PdaExtractingContext<GssNode<int, PdaExtractingGssData>, TaggedEdge<int, string>>;
    using TestPdaConstructingGssNode = GssNode<int, PdaExtractingGssData>;

    using SppfConstructingSimulation = DPDASimulation<int, int, int, TaggedEdge<int, string>, BottomUpSppfConstructingContext<GssNode<int, BottomUpSppfConstructingGssData>>, BottomUpSppfConstructingGssData>;
    using PdaConstructingSimulation =  DPDASimulation<int, int, int, TaggedEdge<int, string>, PdaExtractingContext<GssNode<int, PdaExtractingGssData>, TaggedEdge<int, string>>, PdaExtractingGssData>;

    using PdaConstructingPdvmSimulation = PDVMSimulation<int, int, int, TaggedEdge<int, string>, PdaExtractingContext<GssNode<int, PdaExtractingGssData>, TaggedEdge<int, string>>, PdaExtractingGssData>;

    public sealed class ReferenceEqualityComparer
        : IEqualityComparer, IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Default
            = new ReferenceEqualityComparer();

        private ReferenceEqualityComparer() { }

        public bool Equals(object x, object y)
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
        public static void InitializeSppfProducingSimulation(
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
                new TestCacheProvider<int, int, int, TestSppfConstructingContext, TestSppfConstructingGssNode>();

            var sppf = new Sppf<Extension, TestEdge>();
            var contextProcessor = new BottomUpSppfConstructingContextProcessor<int, int, int, TestEdge>(sppf);

            var simulation = new SppfConstructingSimulation(
                dpda, contextProcessor, cacheProvider, transitionProvider);

            var finalsInternal = new HashSet<TestSppfConstructingContext>();
            simulation.OnFinalState += head => finalsInternal.Add(head.CurrentContext);

            simulate = (startVertex, maxCyclesExpansion) =>
            {
                var start = simulation.Load(startVertex);
                simulation.Run();

                var roots = finalsInternal.Select(final =>
                    sppf.GetOrCreateCompleteNode(new SppfNodeKey<Extension>(start.Extension, final.Extension)));

                var paths =
                    roots.SelectMany(sppfRoot =>
                        ExtractPathsFromSppf(sppfRoot, maxCyclesExpansion)
                            .Select(path => string.Join(" ", path))
                            .Distinct());

                return paths;
            };
        }

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

                using (var writer = new StreamWriter(@"C:\hackathon\contexts.dot"))
                {
                    DumpPda(start, writer);
                }

                return paths;
            };
        }

        public static IEnumerable<IEnumerable<string>> ExtractPathsFromSppf(
            CompleteNode<Extension> root, int maxCyclesExpansion)
        {
            var paths = SppfProcessing.ExtractPaths<int, int, int, TestEdge>(root, maxCyclesExpansion);

            return paths.Select(path => path.Select(edge => edge.Tag));
        }

        private static void DumpSppfInternal(
            ISppfNode current, 
            HashSet<ISppfNode> processed,
            Func<ISppfNode, int> indexer,
            Func<Extension, int> extensionIndexer,
            StreamWriter output)
        {
            if (processed.Contains(current))
            {
                return;
            }

            processed.Add(current);

            var source = indexer(current);

            if (current is TerminalNode<Extension, TestEdge> terminal)
            {
                output.WriteLine($"{source} [label=\"{terminal.Token.Tag}, {extensionIndexer(terminal.Key.LeftExtension)} - {extensionIndexer(terminal.Key.RightExtension)} \"]");
            }
            else if (current is PackedNode<Extension> packed)
            {
                output.WriteLine($"{source} [shape=\"diamond\"]");

                if (packed.Left != null)
                {
                    var target = indexer(packed.Left);
                    output.WriteLine($"{source} -> {target}");
                    DumpSppfInternal(packed.Left, processed, indexer, extensionIndexer, output);
                }

                if (packed.Right != null)
                {
                    var target = indexer(packed.Right);
                    output.WriteLine($"{source} -> {target}");
                    DumpSppfInternal(packed.Right, processed, indexer, extensionIndexer, output);
                }
            }
            else if (current is ProducingNode<Extension> intermediate)
            {
                var key = intermediate.Key;
                var shape = intermediate is CompleteNode<Extension> ? "ellipse" : "box";
                output.WriteLine($"{source} [shape=\"{shape}\" label=\"{extensionIndexer(key.LeftExtension)} - {extensionIndexer(key.RightExtension)}\"]");

                foreach (var child in intermediate.Children)
                {
                    var target = indexer(child);
                    output.WriteLine($"{source} -> {target}");
                    DumpSppfInternal(child, processed, indexer, extensionIndexer, output);
                }
            }
        }

        public static void DumpSppfGraph(IEnumerable<CompleteNode<Extension>> roots, StreamWriter outputWriter)
        {
            var nextIndex = 0;
            var nextExtension = 0;
            var indexes = new Dictionary<ISppfNode, int>(ReferenceEqualityComparer.Default);

            int Indexer(ISppfNode context)
            {
                var exists = indexes.TryGetValue(context, out var index);
                if (!exists)
                {
                    index = nextIndex++;
                    indexes[context] = index;
                }

                return index;
            }

            var extensions = new Dictionary<Extension, int>(ReferenceEqualityComparer.Default);

            int ExtensionIndexer(Extension extension)
            {
                var exists = extensions.TryGetValue(extension, out var index);
                if (!exists)
                {
                    index = nextExtension++;
                    extensions[extension] = index;
                }

                return index;
            }

            var processed = new HashSet<ISppfNode>(ReferenceEqualityComparer.Default);

            outputWriter.WriteLine("digraph Contexts {");

            foreach (var root in roots)
            {
                DumpSppfInternal(root, processed, Indexer, ExtensionIndexer, outputWriter);
            }

            outputWriter.WriteLine("}");
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
