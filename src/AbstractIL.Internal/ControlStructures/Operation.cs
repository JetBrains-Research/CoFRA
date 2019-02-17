using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Statements;

namespace Cofra.AbstractIL.Internal.ControlStructures
{
    [DataContract]
    public class Operation<TPosition>
    {
        protected Operation()
        {
        }

        public Operation(Statement statement, TPosition target)
        {
            Statement = statement;
            Target = target;
        }

        // virtual?
        public virtual Statement Statement { get; }
        public virtual TPosition Target { get; }
    }
}
