using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Cofra.AbstractIL.Internal.Indexing 
{
    [DataContract]
    [KnownType(typeof(DenseBidirectionalIndex<string>))]
    public class DenseIndex<TKey>
    {
        [DataMember]
        protected readonly Dictionary<TKey, int> myInternalIndex;

        [DataMember]
        private readonly DenseIdsProvider myIdsProvider;

        public DenseIndex()
        {
            myInternalIndex = new Dictionary<TKey, int>();
            myIdsProvider = new DenseIdsProvider();
        }

        public virtual int Add(TKey key)
        {
            var exists = myInternalIndex.TryGetValue(key, out var id);

            if (exists)
            {
                throw new ArgumentException("Key already added");
            }

            id = myIdsProvider.NextId();
            myInternalIndex.Add(key, id);

            return id;
        }

        public virtual int? Find(TKey key)
        {
            var exists = myInternalIndex.TryGetValue(key, out var id);

            if (exists)
            {
                return id;
            }

            return null;
        }

        public virtual bool Remove(TKey key)
        {
            var exists = myInternalIndex.TryGetValue(key, out var id);

            if (!exists)
            {
                return false;
            }

            myIdsProvider.FreeId(id);
            myInternalIndex.Remove(key);

            return true;
        }
    }
}
