using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.Types.Ids;

namespace Cofra.Contracts.Messages.Requests
{
    [DataContract]
    public sealed class TaintClassFieldRequest : Request
    {
        [DataMember] public readonly ClassId ClassName;
        [DataMember] public readonly string FieldName;

        public TaintClassFieldRequest(ClassId className, string fieldName)
        {
            ClassName = className;
            FieldName = fieldName;
        }
    }
}
