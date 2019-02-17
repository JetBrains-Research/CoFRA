using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Common.Types
{
    [DataContract]
    public class ClassPropertyReference : ClassMemberReference
    {
        public ClassPropertyReference(Reference owner, string name) : base(owner, name)
        {
        }
        
        public override string ToString()
        {
            return $"ClassPropertyRef:{Owner}-.-{Name}";
        }
    }
}