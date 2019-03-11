using System;
using System.Collections.Generic;
using System.Text;
using PDASimulator.DPDA.Simulation;
using PDASimulator.SimulationCommon;
using QuickGraph;

namespace PDASimulator.Tests.Utils
{
    class TestTransitionProvider : ITransitionProvider<int, TaggedEdge<int, string>>
    {
        private readonly TestGraph myGraph;

        public IEnumerable<TaggedEdge<int, string>> Transitions(int position)
        {
            return myGraph.OutEdges(position);
        }

        public int Target(TaggedEdge<int, string> transition)
        {
            return transition.Target;
        }

        public TestTransitionProvider(TestGraph graph)
        {
            myGraph = graph;
        }
    }
}
