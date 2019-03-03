using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Types.Ids;

namespace Cofra.Contracts.Messages.Requests
{
    [DataContract]
    public sealed class CheckIfTaintedRequest : Request
    {
        [DataMember]
        public readonly List<FieldNamePair> RequestedFields;

        public CheckIfTaintedRequest(IEnumerable<FieldNamePair> requestedFields)
        {
            RequestedFields = requestedFields.ToList();
        }

        public CheckIfTaintedRequest(IEnumerable<(ClassId, string)> requestedFields)
        {
            RequestedFields = requestedFields
                .Select(pair => new FieldNamePair(pair.Item1, pair.Item2))
                .ToList();
        }
    }

    [DataContract]
    public struct FieldNamePair
    {
        [DataMember]
        public readonly ClassId ClassId;

        [DataMember]
        public readonly string FieldName;

        public FieldNamePair(ClassId classId, string fieldName)
        {
            ClassId = classId;
            FieldName = fieldName;
        }
    }
}
