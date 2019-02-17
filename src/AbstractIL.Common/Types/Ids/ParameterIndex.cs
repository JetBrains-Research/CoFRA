using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Common.Types.Ids
{
    [DataContract]
    public class ParameterIndex
    {
        [DataMember]
        public int Value;
        
        public static readonly ParameterIndex ReturnValueIndex = new ParameterIndex(-1);

        public ParameterIndex(int value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is ParameterIndex parameterIndex)
                return parameterIndex.Value == Value;
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}