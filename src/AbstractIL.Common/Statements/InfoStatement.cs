using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.Types;

namespace Cofra.AbstractIL.Common.Statements
{
    [DataContract]
    public class InfoStatement : Statement
    {
        [DataMember]
        public readonly string Info;

        public InfoStatement(Location location, string info) : base(location)
        {
            Info = info;
        }

        public override StatementType Type => StatementType.Info;

        public override string ToString()
        {
            return Location + " " + Info;
        }
    }
}
