using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Types;

namespace Cofra.AbstractIL.Common.ControlStructures
{
    [DataContract]
    public class Continuation
    {
        [DataMember] public readonly List<InstructionId> NextInstructions;

        public Continuation(InstructionId instruction = null)
        {
            NextInstructions = instruction == null ? new List<InstructionId>() : new List<InstructionId> {instruction};
        }

        public void FillWithIndices(Dictionary<int, int> idToIndex)
        {
            foreach (var instruction in NextInstructions)
            {
                
                instruction.ConvertFromCompilerValue(idToIndex);
            }
        }
        
        public bool IsEmpty()
        {
            return NextInstructions.Count == 0;
        }

        public override string ToString()
        {
            return "[" + string.Join(", ", NextInstructions.Select(instruction => instruction.Value.ToString())) + "]";
        }
    }
}