using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Common.Types.InvocationTargets
{
    [DataContract]
    public class LocalVariableTarget : InvocationTarget
    {
        [DataMember] public int Index;

        public LocalVariableTarget(LocalVariableReference localVariableReference)
        {
            Index = localVariableReference.Index;
        }
        
        public override string ToString()
        {
            return $"LocalVar {Index}";
        }
    }
}