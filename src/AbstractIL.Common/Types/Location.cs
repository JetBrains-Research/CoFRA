using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Common.Types
{
    [DataContract]
    public sealed class Location
    {
        public Location(int fileId, int startOffset, int endOffset)
        {
            File = fileId;
            StartOffset = startOffset;
            EndOffset = endOffset;
        }

        [DataMember] public readonly int File;
        [DataMember] public readonly int StartOffset;
        [DataMember] public readonly int EndOffset;

        public override bool Equals(object obj)
        {
            return obj is Location location &&
                   File.Equals(location.File) &&
                   StartOffset == location.StartOffset &&
                   EndOffset == location.EndOffset;
        }

        public override int GetHashCode()
        {
            var hashCode = 1353457620;
            hashCode = hashCode * -1521134295 + File.GetHashCode();
            hashCode = hashCode * -1521134295 + StartOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + EndOffset.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"{File}:{StartOffset}:{EndOffset}";
        }
    }
}