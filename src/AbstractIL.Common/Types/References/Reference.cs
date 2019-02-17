using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Types.Ids;

namespace Cofra.AbstractIL.Common.Types
{
    [DataContract]
    //[KnownType(typeof(ClassId))]
    [KnownType(typeof(ClassMemberReference))]
    [KnownType(typeof(ClassReference))]
    [KnownType(typeof(LocalFunctionReference))]
    [KnownType(typeof(LocalVariableReference))]
    //[KnownType(typeof(NamespaceReference))]
    public abstract class Reference
    {
    }
}