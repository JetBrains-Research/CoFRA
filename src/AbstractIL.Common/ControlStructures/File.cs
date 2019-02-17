using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.Ids;

namespace Cofra.AbstractIL.Common.ControlStructures
{
    [DataContract]
    public class File
    {
        [DataMember] public FileId Id;

        [DataMember] public List<Class> Classes;

        public File(FileId fileId, List<Class> classes = null)
        {
            Id = fileId;
            Classes = classes ?? new List<Class>();
        }

        public override string ToString()
        {
            var x = new StringBuilder();
            foreach (var @class in Classes)
            {
                var classLines = @class.ToString();
                x.Append($"{classLines}\n");
            }

            return x.ToString();
        }
    }
}