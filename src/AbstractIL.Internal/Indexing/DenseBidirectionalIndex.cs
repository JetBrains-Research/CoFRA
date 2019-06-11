using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Cofra.AbstractIL.Internal.Indexing
{
    [DataContract]
    public class DenseBidirectionalIndex<TKey> : DenseIndex<TKey>
    {
        private bool myInitialized = false;

        private Dictionary<int, TKey> myInvertedIndex;
        private Dictionary<int, TKey> InvertedIndex
        {
            get
            {
                InitializeIfNeeded();

                return myInvertedIndex;
            }
        }

        private void InitializeIfNeeded()
        {
            if (!myInitialized)
            {
                myInvertedIndex = myInternalIndex.ToDictionary(pair => pair.Value, pair => pair.Key);
                myInitialized = true;
            }
        }

        public override int Add(TKey key)
        {
            InitializeIfNeeded();
            
            var id = base.Add(key);
            InvertedIndex.Add(id, key);

            return id;
        }

        public override bool Remove(TKey key)
        {
            var possibleId = base.Find(key);

            if (possibleId == null)
            {
                return false;
            }

            var id = possibleId.Value;
            InvertedIndex.Remove(id);
            base.Remove(key);

            return true;
        }

        public bool FindKey(int id, out TKey key)
        {
            var exists = InvertedIndex.TryGetValue(id, out key);

            return exists;
        }

        public TKey GetKey(int id)
        {
            return InvertedIndex[id];
        }

        public int FindOrAdd(TKey key)
        {
            var existing = Find(key);

            return existing ?? Add(key);
        }
    }
}
