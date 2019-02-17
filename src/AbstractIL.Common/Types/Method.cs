using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using Cofra.AbstractIL.Common.ControlStructures;
using Cofra.AbstractIL.Common.Types.Ids;

namespace Cofra.AbstractIL.Common.Types
{
    [DataContract]
    public class Method
    {
        /// <summary>
        /// Multiple base methods case: method implements interface and overrides base defaultType method
        /// </summary> 
        [DataMember] public readonly List<MethodId> BaseMethods = new List<MethodId>();
        [DataMember] public readonly MethodId Id;
        [DataMember] public readonly Continuation InitialInstructions;
        [DataMember] public readonly List<Instruction> Instructions;

        [DataMember] public readonly List<Reference> Variables = new List<Reference>();
        [DataMember] public readonly List<Method> LocalMethods = new List<Method>();

        public Method(MethodId id, List<Instruction> instructions, IEnumerable<InstructionId> initialInstructions)
        {
            Id = id;
            Instructions = instructions ?? throw new ArgumentNullException();
            var cont = new Continuation();
            cont.NextInstructions.AddRange(initialInstructions ?? throw new ArgumentNullException());
            InitialInstructions = cont;
        }

        public Method(MethodId id, InstructionBlock instructionBlock = null)
        {
            Id = id;
            Instructions = new List<Instruction>();
            InitialInstructions = new Continuation();
            if (instructionBlock != null) FillWithInstructions(instructionBlock);
        }

        public void FillWithInstructions(InstructionBlock instructionBlock)
        {
            if (instructionBlock == null) throw new ArgumentNullException();
            if (!instructionBlock.AreContinuationsEmpty())
                throw new Exception("Method has continuations");
            Instructions.AddRange(instructionBlock.Instructions);
            InitialInstructions.NextInstructions.AddRange(instructionBlock.InitialInstructions);
        }
        
        public void AddBase(MethodId methodId)
        {
            BaseMethods.Add(methodId);
        }
        
        public void FillWithIndices()
        {
            var idToIndex = new Dictionary<int, int>();
            for (var i = 0; i < Instructions.Count; i++) idToIndex.Add(Instructions[i].Id.CompilerValue, i);
            
            Instructions.ForEach(instruction => instruction.Continuation.FillWithIndices(idToIndex));
            InitialInstructions.FillWithIndices(idToIndex);
        }
        
        public override string ToString()
        {
            if (Instructions.Count == 0 && LocalMethods.Count == 0) return $"Method- {Id}: EMPTY\n";
            
            var x = new StringBuilder();
            x.Append($"Method- {Id}:\n");
            x.Append($"Starts:{InitialInstructions}\n");

            if (LocalMethods.Count > 0)
            {
                x.Append($"Local Methods:\n");
                foreach (var method in LocalMethods)
                {
                    var methodLines = method.ToString();
                    x.Append($"{methodLines}\n");
                }
            }

            if (Instructions.Count > 0)
            {
                x.Append($"Instructions:\n");
                foreach (var instruction in Instructions)
                {
                    x.Append($"{instruction}\n");
                }
            }


            return x.ToString();
        }
    }
}