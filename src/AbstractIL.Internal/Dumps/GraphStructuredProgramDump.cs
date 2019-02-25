using System.Collections.Generic;
using System.Runtime.Serialization;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Indexing;
using Cofra.AbstractIL.Internal.Types;
using Cofra.AbstractIL.Internal.Types.Primaries;

namespace Cofra.AbstractIL.Internal.Dumps
{
    [DataContract]
    [KnownType(typeof(GraphStructuredProgramDump<int>))]
    public struct GraphStructuredProgramDump<TNode>
    {
        [DataMember] 
        public readonly DenseBidirectionalIndex<string> FilesIndex;

        [DataMember] 
        public readonly DenseBidirectionalIndex<string> ClassesIndex;

        [DataMember] 
        public readonly DenseBidirectionalIndex<string> MethodsIndex;

        [DataMember] 
        public readonly DenseBidirectionalIndex<string> ClassFieldsIndex;

        [DataMember]
        public readonly Dictionary<ResolvedClassId, ResolvedClass<TNode>> Classes;

        [DataMember]
        public readonly Dictionary<int, File> Files;

        [DataMember]
        public readonly List<OperationEdge<TNode>> Edges;

        public GraphStructuredProgramDump(
            List<OperationEdge<TNode>> edges,
            DenseBidirectionalIndex<string> filesIndex,
            DenseBidirectionalIndex<string> classesIndex,
            DenseBidirectionalIndex<string> methodsIndex,
            DenseBidirectionalIndex<string> classFieldsIndex,
            Dictionary<ResolvedClassId, ResolvedClass<TNode>> classes,
            Dictionary<int, File> files)
        {
            Edges = edges;
            FilesIndex = filesIndex;
            ClassesIndex = classesIndex;
            MethodsIndex = methodsIndex;
            ClassFieldsIndex = classFieldsIndex;
            Classes = classes;
            Files = files;
        }
    }
}
