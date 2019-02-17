using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Common.Types.InvocationTargets
{
    [DataContract]
    public class ClassMethodTarget : InvocationTarget
    {
        [DataMember] public Reference TargetClass;
        [DataMember] public string TargetMethod;

        public ClassMethodTarget(ClassMethodReference classMethodReference)
        {
            TargetClass = classMethodReference.Owner;
            TargetMethod = classMethodReference.Name;
        }
        
        public override string ToString()
        {
            return $"{TargetClass} Method:{TargetMethod}";
        }
    }
}