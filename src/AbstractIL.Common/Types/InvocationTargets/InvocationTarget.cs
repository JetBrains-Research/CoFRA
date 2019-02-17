using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Common.Types.InvocationTargets
{
    [DataContract]
    [KnownType(typeof(ClassFieldTarget))]
    [KnownType(typeof(ClassMethodTarget))]
    [KnownType(typeof(ClassPropertyTarget))]
    [KnownType(typeof(LocalFunctionTarget))]
    [KnownType(typeof(LocalVariableTarget))]
    public abstract class InvocationTarget
    {
    }
}