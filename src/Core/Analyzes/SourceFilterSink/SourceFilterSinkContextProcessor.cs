using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Types.Secondaries;
using PDASimulator.DataStructures.GSS;
using PDASimulator.Payloads;
using PDASimulator.SimulationCommon;

namespace Cofra.Core.Analyzes.SourceFilterSink
{
    using Node = Int32;
    using State = SecondaryEntity;
    using Transition = OperationEdge<Int32>;

    public sealed class SourceFilterSinkContextProcessor :
        PdaExtractingContextProcessor<State, StackSymbol, Node, Transition>
    {
    }
}
