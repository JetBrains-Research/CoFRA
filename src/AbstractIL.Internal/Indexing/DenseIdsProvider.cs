using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Internal.Indexing 
{
    [DataContract]
    public class DenseIdsProvider
    {
        [DataMember]
        private int myNextId;

        [DataMember]
        private readonly Stack<int> myFreeIds;

        public DenseIdsProvider()
        {
            myNextId = 0;
            myFreeIds = new Stack<int>();
        }

        public int NextId()
        {
            if (myFreeIds.Count > 0)
            {
                return myFreeIds.Pop();
            }

            return myNextId++;
        }

        public void FreeId(int id)
        {
            myFreeIds.Push(id);
        }
    }
}
