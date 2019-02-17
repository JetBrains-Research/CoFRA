using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Common.Types
{
    [DataContract]
    public class ClassMethodReference : ClassMemberReference
    {
        public ClassMethodReference(Reference owner, string name) : base(owner, name)
        {
        }
        
        public override string ToString()
        {
            return $"ClassMethodRef:{Owner}-.-{Name}";
        }
    }
}