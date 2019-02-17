using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Types.Ids;

namespace Cofra.AbstractIL.Common.Types.InvocationTargets
{
    [DataContract]
    public class LocalFunctionTarget : InvocationTarget
    {
        [DataMember] public MethodId TargetFunction;

        public LocalFunctionTarget(LocalFunctionReference localFunctionReference)
        {
            TargetFunction = localFunctionReference.MethodId;
        }
        
        public override string ToString()
        {
            return $"LocalFun:{TargetFunction}";
        }
    }
}