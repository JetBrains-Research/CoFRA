using System.Runtime.Serialization;

namespace Cofra.Contracts.Messages.Responses
{
    [DataContract]
    [KnownType(typeof(FailureResponse))]
    [KnownType(typeof(SuccessResponse))]
    [KnownType(typeof(StatementsTraceResponse))]
    [KnownType(typeof(TaintedFieldsResponse))]
    public abstract class Response
    {
    }
}
