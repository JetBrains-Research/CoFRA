using System;
using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Common.Types
{
    [DataContract]
    [KnownType(typeof(ClassFieldReference))]
    [KnownType(typeof(ClassMethodReference))]
    [KnownType(typeof(ClassPropertyReference))]
    public class ClassMemberReference : Reference
    {
        [DataMember] public Reference Owner;
        [DataMember] public string Name;

        public ClassMemberReference(Reference owner, string name)
        {
            Owner = owner;
            Name = name;
        }
    }
}