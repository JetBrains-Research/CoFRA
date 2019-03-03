using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Cofra.Contracts.Messages.Responses
{
    [DataContract]
    public sealed class TaintedFieldsResponse : Response
    {
        [DataMember]
        public readonly List<bool> TaintingFlags;

        public TaintedFieldsResponse(IEnumerable<bool> taintingFlags)
        {
            TaintingFlags = taintingFlags.ToList();
        }
    }
}
