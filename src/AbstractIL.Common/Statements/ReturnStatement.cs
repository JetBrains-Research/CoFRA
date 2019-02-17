using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.Ids;

namespace Cofra.AbstractIL.Common.Statements
{
    [DataContract]
    public class ReturnStatement : AssignmentStatement
    {
        public ReturnStatement(Location location, Reference source)
            : base(location, source, new LocalVariableReference(ParameterIndex.ReturnValueIndex.Value))
        {
        }
    }
}