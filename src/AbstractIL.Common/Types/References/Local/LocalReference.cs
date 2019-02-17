using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Types.Ids;

namespace Cofra.AbstractIL.Common.Types
{
    [DataContract]
    public class LocalVariableReference : Reference
    {
        [DataMember] public int Index;

        [DataMember] public ClassId DefaultType;

        public LocalVariableReference(int index, ClassId defaultType = null)
        {
            Index = index;
            DefaultType = defaultType;
        }
        
        public override string ToString()
        {
            return $"LocalVarRef:{Index}";
        }
    }
}