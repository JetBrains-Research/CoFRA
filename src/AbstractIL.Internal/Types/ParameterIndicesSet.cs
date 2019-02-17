using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.Types.Ids;

namespace Cofra.AbstractIL.Internal.Types
{
    [DataContract]
    public sealed class ParameterIndicesSet : ParameterIndex
    {
        [DataMember]
        private readonly List<int> myIndices;

        public IEnumerable<int> Indices => myIndices;

        public ParameterIndicesSet(IEnumerable<int> indices) : base(default(int))
        {
            myIndices = new List<int>(indices);
            Value = myIndices[0];
        }
    }
}
