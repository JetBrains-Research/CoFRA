using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Common.ControlStructures
{
    [DataContract]
    public struct FileId
    {
        [DataMember] public string Value;

        public FileId(string name)
        {
            Value = name;
        }
    }
}