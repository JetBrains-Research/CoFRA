using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using Cofra.AbstractIL.Internal.Dumps;
using Cofra.AbstractIL.Internal.Types;

namespace Cofra.AbstractIL.Internal.ControlStructures
{
    [DataContract]
    public class File
    {
        [DataMember]
        public readonly int GlobalId;

        private IImmutableSet<ResolvedFullMethodId> myOwned = 
            ImmutableHashSet<ResolvedFullMethodId>.Empty;

        [DataMember]
        private HashSet<ResolvedFullMethodId> SerializedOwned
        {
            get => new HashSet<ResolvedFullMethodId>(myOwned);
            set => myOwned = ImmutableHashSet<ResolvedFullMethodId>.Empty.Union(value);
        }

        public IEnumerable<ResolvedFullMethodId> Owned => myOwned;

        public File(int id)
        {
            GlobalId = id;
        }

        public File(int id, IEnumerable<ResolvedFullMethodId> owned)
        {
            GlobalId = id;
            myOwned = myOwned.Union(owned);
        }

        public void Update(
            IEnumerable<ResolvedFullMethodId> methods, 
            out IImmutableSet<ResolvedFullMethodId> removed,
            out IImmutableSet<ResolvedFullMethodId> updated, 
            out IImmutableSet<ResolvedFullMethodId> added)
        {
            //TODO: make set of owned be not null
            if (myOwned == null)
            {
                myOwned = ImmutableHashSet<ResolvedFullMethodId>.Empty;
            }

            var newMethods = ImmutableHashSet<ResolvedFullMethodId>.Empty.Union(methods);

            removed = myOwned.Except(newMethods);
            updated = myOwned.Intersect(newMethods);
            added = newMethods.Except(myOwned);

            myOwned = newMethods;
        }

        public FileDump Dump()
        {
            return new FileDump(Owned);
        }
    }
}
