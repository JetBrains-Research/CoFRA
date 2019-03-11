using NUnit.Framework;
using PDASimulator.DataStructures.GSS;
using PDASimulator.Payloads;
using PDASimulator.Tests.Utils;
using QuickGraph;

namespace PDASimulator.Tests.BlackBox
{
    [TestFixture]
    public static class PDVM
    {
        [Test]
        public static void FindSpecificTokenTest()
        {
            TestHelper.InitializePdaProducingPdvmSimulation<MyPDVM>(
                out var graph, out var simulate);

            void Edge(int source, int target, string tag) =>
                graph.AddVerticesAndEdge(new TaggedEdge<int, string>(source, target, tag));

            Edge(0, 1, "a");
            Edge(1, 2, "s");
            Edge(2, 3, "e");
            Edge(3, 4, "e");
            Edge(4, 2, "e");
            Edge(3, 5, "b");
            Edge(5, 6, "k");

            var paths = simulate(0, 2);
        }

        private class MyPDVM : TestPDVM
        {
            public override void Action(int state, GssNode<int, PdaExtractingGssData> stack)
            {
                if (state == 2)
                {
                    Finish();
                }
            }


            public override void Step(
                int state, 
                GssNode<int, PdaExtractingGssData> stack, 
                int position,
                TaggedEdge<int, string> currentTransition)
            {
                var token = currentTransition.Tag;

                switch (token)
                {
                    case "a":
                        Push(1, 0);
                        break;
                    case "b":
                        if (stack.Symbol == 0)
                        {
                            Pop(1);
                        }
                        break;
                    case "s":
                        Accept(1);
                        break;
                    case "e":
                        Skip(1);
                        break;
                    case "k":
                        Accept(2);
                        //Finish();
                        break;
                }
            }
        }
    }
}
