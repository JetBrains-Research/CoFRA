using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Types.AnalysisSpecific;

namespace Cofra.Contracts.Messages.Requests
{
    [DataContract]
    public sealed class AnalysisResultsRequest : Request
    {
        [DataMember(Name = "Analysis")] public readonly AnalysisType Analysis;
        [DataMember(Name = "File")] public readonly int FileIndex;

        public AnalysisResultsRequest(AnalysisType analysis, int fileIndex)
        {
            Analysis = analysis;
            FileIndex = fileIndex;
        }
    }
}
