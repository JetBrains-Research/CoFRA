using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Types;

namespace Cofra.AbstractIL.Common.Statements
{
    [DataContract]
    [KnownType(typeof(AssignmentStatement))]
    [KnownType(typeof(CatchStatement))]
    [KnownType(typeof(ExceptionStatement))]
    [KnownType(typeof(FinallyStatement))]
    [KnownType(typeof(InvocationStatement))]
    [KnownType(typeof(ThrowStatement))]
    [KnownType(typeof(InfoStatement))]
    public abstract class Statement
    {
        protected Statement(Location location)
        {
            Location = location;
        }

        /// <summary>
        ///     Location of statement in source code
        /// </summary>
        [DataMember] public readonly Location Location;

        public abstract StatementType Type { get; }

        public override bool Equals(object obj)
        {
            return obj is Statement statement &&
                   (statement.Location == null && Location == null ||
                    statement.Location != null && Location != null &&
                    statement.Location.Equals(Location));
        }

        protected bool Equals(Statement other)
        {
            return Location.Equals(other.Location);
        }

        public override int GetHashCode()
        {
            return (Location != null ? Location.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}