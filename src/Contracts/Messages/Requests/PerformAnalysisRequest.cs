using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.AnalysisSpecific;

namespace Cofra.Contracts.Messages.Requests
{
    [DataContract]
    public sealed class PerformAnalysisRequest : Request
    {
        [DataMember(Name = "Analysis")] public readonly AnalysisType Analysis;

        public PerformAnalysisRequest(AnalysisType analysis)
        {
            Analysis = analysis;
        }
    }
}
