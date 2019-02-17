using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Types.Ids;

namespace Cofra.AbstractIL.Common.Types
{
    [DataContract]
    public class ClassFieldReference : ClassMemberReference
    {
        [DataMember]
        public readonly ClassId DefaultClassType;

        public ClassFieldReference(Reference owner, string name, ClassId defaultType = null) : base(owner, name)
        {
            DefaultClassType = defaultType;
        }
        
        public override string ToString()
        {
            return $"ClassFieldRef:{Owner}-.-{Name}";
        }
    }
}