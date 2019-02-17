using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Types.Ids;

namespace Cofra.AbstractIL.Common.Types
{
    [DataContract]
    public class LocalFunctionReference : Reference
    {
        [DataMember] public readonly MethodId MethodId;

        public LocalFunctionReference(MethodId methodId)
        {
            MethodId = methodId;
        }
        
        public override string ToString()
        {
            return $"LocalFunRef:{MethodId}";
        }
    }
}