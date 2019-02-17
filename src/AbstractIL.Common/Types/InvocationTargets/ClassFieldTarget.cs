using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Common.Types.InvocationTargets
{
    [DataContract]
    public class ClassFieldTarget : InvocationTarget
    {
        [DataMember] public Reference TargetClass;
        [DataMember] public string TargetField;

        public ClassFieldTarget(ClassFieldReference classFieldReference)
        {
            TargetClass = classFieldReference.Owner;
            TargetField = classFieldReference.Name;
        }

        public override string ToString()
        {
            return $"{TargetClass}-.-{TargetField}";
        }
    }
}