using System;
using System.Collections.Generic;
using System.Text;
using Cofra.AbstractIL.Internal;
using Cofra.AbstractIL.Internal.ControlStructures;
using PDASimulator.SimulationCommon;

namespace Cofra.Core.Analyzes.PDVM
{
    using Node = Int32;

    public class TransitionProvider 
        : ITransitionProvider<Node, OperationEdge<Node>>
    {
        private readonly GraphStructuredProgram<Node> myProgram;

        public TransitionProvider(GraphStructuredProgram<Node> program)
        {
            myProgram = program;
        }

        public IEnumerable<OperationEdge<Node>> Transitions(int position)
        {
            return myProgram.OutEdges(position);
        }

        public Node Target(OperationEdge<Node> transition)
        {
            return transition.Target;
        }
    }
}
