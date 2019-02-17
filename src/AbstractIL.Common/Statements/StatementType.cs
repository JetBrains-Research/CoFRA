using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Common.Statements
{
    [DataContract]
    public enum StatementType
    {
        Invocation,
        Finally,
        Catch,
        Throw,
        Exception,
        Assignment,
        Internal,
        Info
    }
}