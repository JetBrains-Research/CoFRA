using System;
using System.Runtime.Serialization;
using Cofra.AbstractIL.Internal.Indexing;

namespace Cofra.AbstractIL.Internal.Dumps
{
    using Node = Int32;

    [DataContract]
    public struct GraphStructuredProgramBuilderState
    {
        [DataMember]
        public readonly GraphStructuredProgramDump<Node> Program;

        [DataMember]
        public readonly DenseIdsProvider NodeIdsProvider;

        public GraphStructuredProgramBuilderState(
            GraphStructuredProgramDump<Node> program,
            DenseIdsProvider nodeIdsProvider)
        {
            Program = program;
            NodeIdsProvider = nodeIdsProvider;
        }
    }
}
