using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Cofra.AbstractIL.Internal.Types.Secondaries;

namespace Cofra.AbstractIL.Internal.Types.Primaries
{
    [DataContract]
    [KnownType(typeof(ResolvedMethod<int>))]
    public sealed class ResolvedMethod<TNode> : PrimaryEntity, IMethodHolder<TNode>, IInvokable<TNode>
    {
        [DataMember]
        public readonly TNode Start;

        [DataMember]
        public readonly TNode Final;

        [DataMember]
        private readonly List<TNode> myOwned;

        [DataMember] 
        private readonly Dictionary<int, ResolvedLocalVariable> myVariables;

        [DataMember] 
        private readonly Dictionary<ResolvedMethodId, ResolvedMethod<TNode>> myLocalFunctions;

        private bool myInvoked; 

        public TNode EntryPoint => Start;
        public IReadOnlyDictionary<int, ResolvedLocalVariable> Variables => myVariables;

        public bool Invoked => myInvoked;

        public void MarkAsInvoked()
        {
            myInvoked = true;
        }

        public void ResetInvocationMarker()
        {
            myInvoked = false;
        }

        public IReadOnlyDictionary<ResolvedMethodId, ResolvedMethod<TNode>> Methods => myLocalFunctions;

        public HashSet<SecondaryEntity> AdditionalVariables;

        public ResolvedMethod(ResolvedMethodId id, TNode start, TNode final)
        {
            Start = start;
            Final = final;

            myOwned = new List<TNode> {Start, Final};
            myVariables = new Dictionary<int, ResolvedLocalVariable>();
            myLocalFunctions = new Dictionary<ResolvedMethodId, ResolvedMethod<TNode>>();
            AdditionalVariables = new HashSet<SecondaryEntity>();
        }

        public void ResetAdditionalVariables()
        {
            AdditionalVariables = new HashSet<SecondaryEntity>();
        }

        public void AddLocalVariable(ResolvedLocalVariable variable)
        {
            myVariables.Add(variable.LocalId, variable);
        }

        public void RemoveLocalVariable(int id)
        {
            myVariables.Remove(id);
        }

        public void AddOwnedNode(TNode node)
        {
            myOwned.Add(node);
        }

        public void Clear()
        {
            myOwned.Clear();
            myVariables.Clear();
            myLocalFunctions.Clear();
            ResetAdditionalVariables();

            myOwned.Add(Start);
            myOwned.Add(Final);
        }

        public IEnumerable<TNode> GetOwnedNodes()
        {
            return myOwned;
        }

        public ResolvedMethod<TNode> FindMethodInFullHierarchy(ResolvedMethodId id)
        {
            Methods.TryGetValue(id, out var method);

            return method;
        }

        public ResolvedMethod<TNode> FindLocalMethod(ResolvedMethodId id)
        {
            return FindMethodInFullHierarchy(id);
        }

        public IEnumerable<ResolvedMethod<TNode>> CollectAllInternalMethods()
        {
            return Methods.Values.Union(
                Methods.Values.SelectMany(
                    method => method.CollectAllInternalMethods()));
        }

        public IEnumerable<(ResolvedMethodId, ResolvedMethod<TNode>)> CollectInternalMethods()
        {
            return myLocalFunctions.Select(pair => (pair.Key, pair.Value));
        }

        public void AddMethod(ResolvedMethodId id, ResolvedMethod<TNode> method)
        {
            var exists = myLocalFunctions.ContainsKey(id);

            if (!exists)
            {
                myLocalFunctions.Add(id, method);
            }
            else
            {
                myLocalFunctions[id] = method;
            }
        }

        public bool RemoveMethod(ResolvedMethodId id)
        {
            return myLocalFunctions.Remove(id);
        }

        public void StoreAdditionalVariable(SecondaryEntity variable)
        {
            AdditionalVariables.Add(variable);
        }
    }
}
