using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Common.Types.Ids
{
    [DataContract]
    public sealed class ClassId// : Reference
    {
        [DataMember] public readonly string Value;

        public ClassId(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
