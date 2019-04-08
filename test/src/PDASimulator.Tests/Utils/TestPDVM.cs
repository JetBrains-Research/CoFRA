using System;
using System.Collections.Generic;
using System.Text;
using PDASimulator.DataStructures.GSS;
using PDASimulator.Payloads;
using PDASimulator.PDVM;
using PDASimulator.SimulationCommon;
using QuickGraph;

namespace PDASimulator.Tests.Utils
{
    using TestEdge = TaggedEdge<int, string>;
    using TestContext = PdaExtractingContext<GssNode<int, EmptyGssData>, TaggedEdge<int, string>>;

    public abstract class TestPDVM : 
        PDVM<int, int, int, TestEdge, TestContext, EmptyGssData>
    {
        private readonly HashSet<TestContext> myFinals;

        public IReadOnlyCollection<TestContext> Finals => myFinals;

        protected TestPDVM()
        {
            myFinals = new HashSet<TestContext>();
        }

        protected override void OnFinish(
            Head<int, int, TestContext, GssNode<int, EmptyGssData>> head, 
            TestEdge edge)
        {
            myFinals.Add(head.CurrentContext);
        }
    }
}
