using System.Collections.Generic;
using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.ControlStructures;

namespace Cofra.Contracts.Messages.Requests
{
    [DataContract]
    public sealed class UpdateFileRequest : Request
    {
        [DataMember] public readonly string Name;
        [DataMember] public readonly File File;

        public UpdateFileRequest(string name, File file)
        {
            Name = name;
            File = file;
        }
    }
}
