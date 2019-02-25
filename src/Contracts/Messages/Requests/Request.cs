using System.Runtime.Serialization;

namespace Cofra.Contracts.Messages.Requests
{
    [DataContract]
    [KnownType(typeof(UpdateFileRequest))]
    [KnownType(typeof(UpdateMethodRequest))]
    [KnownType(typeof(TerminatingRequest))]
    [KnownType(typeof(PerformAnalysisRequest))]
    [KnownType(typeof(AnalysisResultsRequest))]
    [KnownType(typeof(TaintClassFieldRequest))]
    public abstract class Request
    {
    }
}
