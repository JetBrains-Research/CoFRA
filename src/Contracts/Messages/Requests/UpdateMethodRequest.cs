using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Types;

namespace Cofra.Contracts.Messages.Requests
{
    [DataContract]
    public sealed class UpdateMethodRequest : Request
    {
        [DataMember(Name = "Method")] public readonly Method Method;

        public UpdateMethodRequest(Method method)
        {
            Method = method;
        }
    }
}
