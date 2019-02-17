using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Common.Types.Ids
{
    [DataContract]
    public struct MethodId
    {
        [DataMember] public readonly string Value;

        public MethodId(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}