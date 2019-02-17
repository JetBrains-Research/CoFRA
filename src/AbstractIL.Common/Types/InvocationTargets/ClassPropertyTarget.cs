using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Common.Types.InvocationTargets
{
    [DataContract]
    public class ClassPropertyTarget : InvocationTarget
    {
        [DataMember] public Reference TargetClass;
        [DataMember] public string TargetProperty;

        public ClassPropertyTarget(ClassPropertyReference classFieldReference)
        {
            TargetClass = classFieldReference.Owner;
            TargetProperty = classFieldReference.Name;
        }
        
        public override string ToString()
        {
            return $"{TargetClass}-.-{TargetProperty}";
        }
    }
}