using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.Types.Ids;

namespace Cofra.AbstractIL.Common.Types
{ 
    [DataContract]
    public class Class
    {
        [DataMember]
        public readonly ClassId ClassId;

        [DataMember]
        public readonly List<Method> Methods;

        [DataMember]
        public readonly List<string> Fields;
        
        [DataMember]
        public ClassId BaseClass;
        

        public Class(ClassId classId, List<Method> methods = null, List<string> fields = null)
        {
            Methods = methods ?? new List<Method>();
            ClassId = classId;
            Fields = fields ?? new List<string>();
        }
        
        public override string ToString()
        {
            var x = new StringBuilder();
            x.Append(ClassId + "\n");
            foreach (var method in Methods)
            {
                var methodLines = method.ToString();
                x.Append($"{methodLines}\n");
            }

            return x.ToString();
        }
    }
}
