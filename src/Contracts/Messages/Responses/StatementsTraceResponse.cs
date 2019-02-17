using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Statements;

namespace Cofra.Contracts.Messages.Responses
{
    [DataContract]
    public sealed class StatementsTraceResponse : SuccessResponse
    {
        [DataMember(Name = "Trace")] public readonly List<List<Statement>> Traces;

        public StatementsTraceResponse(IEnumerable<IEnumerable<Statement>> traces)
        {
            Traces = new List<List<Statement>>(
                traces.Select(trace => new List<Statement>(trace)));
        }
    }
}
