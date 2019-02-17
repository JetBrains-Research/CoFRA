using System.Runtime.Serialization;

namespace Cofra.Contracts.Messages.Requests
{
    [DataContract]
    [KnownType(typeof(UpdateFileRequest))]
    [KnownType(typeof(UpdateMethodRequest))]
    [KnownType(typeof(TerminatingRequest))]
    [KnownType(typeof(PerformAnalysisRequest))]
    [KnownType(typeof(AnalysisResultsRequest))]
    public abstract class Request
    {
    }
}
