using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Common.ControlStructures
{
    [DataContract]
    public class InstructionId
    {
        [DataMember] public int Value;
        public readonly int CompilerValue;

        public InstructionId(int value)
        {
            CompilerValue = value;
        }

        public void ConvertFromCompilerValue(Dictionary<int, int> idToIndex)
        {
            Value = idToIndex[CompilerValue];
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}