using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Cofra.AbstractIL.Internal.Types
{
    [DataContract]
    public sealed class ResolvedClass<TNode> : IMethodHolder<TNode>
    {
        [DataMember]
        public readonly ResolvedClassId Id;

        [DataMember] 
        public ResolvedClassId Base;

        [DataMember]
        private readonly Dictionary<ResolvedMethodId, ResolvedMethod<TNode>> myMethods;

        [DataMember]
        private readonly Dictionary<int, ResolvedClassField> myFields;

        public IEnumerable<ResolvedClassField> Fields =>
            myFields.Values.Concat(ResolvedBase?.Fields ?? Enumerable.Empty<ResolvedClassField>());

        private ResolvedClass<TNode> ResolvedBase;

        public ResolvedClass(ResolvedClassId id, ResolvedClassId _base = null)
        {
            Id = id;
            Base = _base;
            myMethods = new Dictionary<ResolvedMethodId, ResolvedMethod<TNode>>();
            myFields = new Dictionary<int, ResolvedClassField>();
        }

        public void SetBase(ResolvedClass<TNode> resolvedBase)
        {
            ResolvedBase = resolvedBase;
        }

        public void SetBaseId(ResolvedClassId baseId)
        {
            Base = baseId;
        }

        public ResolvedMethod<TNode> FindMethodInFullHierarchy(ResolvedMethodId id)
        {
            var exists = myMethods.TryGetValue(id, out var method);

            if (!exists)
            {
                method = ResolvedBase?.FindMethodInFullHierarchy(id);
            }

            return method;
        }

        public ResolvedMethod<TNode> FindLocalMethod(ResolvedMethodId id)
        {
            myMethods.TryGetValue(id, out var method);
            return method;
        }

        public IEnumerable<(ResolvedMethodId, ResolvedMethod<TNode>)> Methods()
        {
            return myMethods.Select(pair => (pair.Key, pair.Value));
        }

        public ResolvedClassField FindField(int id)
        {
            var exists = myFields.TryGetValue(id, out var field);

            if (!exists)
            {
                field = ResolvedBase?.FindField(id);
            }

            return field;
        }

        public ResolvedClassField GetOrCreateField(int fieldId)
        {
            var field = FindField(fieldId);

            if (field == null)
            {
                field = new ResolvedClassField(Id, fieldId);
                field.ResetPropagatedTypes();
                myFields.Add(fieldId, field);
            }

            return field;
        }

        public IEnumerable<ResolvedMethod<TNode>> CollectAllInternalMethods()
        {
            return myMethods.Values.Union(
                myMethods.Values.SelectMany(
                    method => method.CollectAllInternalMethods()));
        }

        public IEnumerable<(ResolvedMethodId, ResolvedMethod<TNode>)> CollectInternalMethods()
        {
            return myMethods.Select(pair => (pair.Key, pair.Value));
        }

        public void AddMethod(ResolvedMethodId id, ResolvedMethod<TNode> method)
        {
            var exists = myMethods.ContainsKey(id);

            if (!exists)
            {
                myMethods.Add(id, method);
            }
            else
            {
                myMethods[id] = method;
            }
        }

        public bool RemoveMethod(ResolvedMethodId id)
        {
            return myMethods.Remove(id);
        }
    }
}
