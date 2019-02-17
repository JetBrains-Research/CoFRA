using System.Runtime.Serialization;

namespace Cofra.Contracts.Messages.Responses
{
    [DataContract]
    [KnownType(typeof(FailureResponse))]
    [KnownType(typeof(SuccessResponse))]
    [KnownType(typeof(StatementsTraceResponse))]
    public abstract class Response
    {
    }
}
