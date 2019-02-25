using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Cofra.AbstractIL.Internal.Types.Primaries;
using Cofra.AbstractIL.Internal.Utils;

namespace Cofra.AbstractIL.Internal.Types.Secondaries
{
    [DataContract]
    [KnownType(typeof(ResolvedLocalVariable))]
    [KnownType(typeof(ResolvedClassField))]
    [KnownType(typeof(ResolvedObjectField))]
    [KnownType(typeof(ResolvedObjectMethodReference<int>))]
    public abstract class SecondaryEntity : Entity
    {
        protected Lazy<TransitiveClosedNode<PrimaryEntity, SecondaryEntity>> TopLevelClosure;
        protected Lazy<TransitiveClosedNode<PrimaryEntity, SecondaryEntity>> BottomLevelClosure;

        //TODO: assert: TopLevelClosure != null
        public IEnumerable<PrimaryEntity> CollectedPrimaries =>
            TopLevelClosure == null ? 
                Enumerable.Empty<PrimaryEntity>() : 
                TopLevelClosure.Value.Collected.Union(BottomLevelClosure.Value.Collected).Distinct();

        public IEnumerable<(SecondaryEntity, bool)> AllRecipients =>
            TopLevelClosure.Value.Recipients.Select(node => (node.Owner, true))
                .Union(BottomLevelClosure.Value.Recipients.Select(node => (node.Owner, false)))
                .Distinct();

        private Lazy<MySubscribersManager> mySubscriptions;
        public MySubscribersManager Subscriptions => mySubscriptions.Value;

        public virtual void ResetPropagatedTypes()
        {
            mySubscriptions = new Lazy<MySubscribersManager>();

            TopLevelClosure = new Lazy<TransitiveClosedNode<PrimaryEntity, SecondaryEntity>>(() =>
            {
                var node = new TransitiveClosedNode<PrimaryEntity, SecondaryEntity>();

                node.Owner = this;
                node.OnNewDataAdded += OnNewTopEntityAdded;

                return node;
            });
            BottomLevelClosure = new Lazy<TransitiveClosedNode<PrimaryEntity, SecondaryEntity>>(() =>
            {
                var node = new TransitiveClosedNode<PrimaryEntity, SecondaryEntity>();

                node.Owner = this;
                node.OnNewDataAdded += OnNewBottomEntityAdded;

                return node;
            });
        }

        public bool HasSubscriptions()
        {
            return mySubscriptions.IsValueCreated;
        }

        public virtual void DropTransitiveClosure()
        {
            if (TopLevelClosure?.IsValueCreated == true)
            {
                TopLevelClosure?.Value.DropConnections();
            }

            if (BottomLevelClosure?.IsValueCreated == true)
            {
                BottomLevelClosure?.Value.DropConnections();
            }

            mySubscriptions = null;
        }

        public virtual void DropAllCollectedPrimaries()
        {
            TopLevelClosure = null;
            BottomLevelClosure = null;
        }

        public virtual void AddPrimary(PrimaryEntity entity)
        {
            TopLevelClosure.Value.PutData(entity);
            BottomLevelClosure.Value.PutData(entity);
        }

        public virtual bool PropagateUp(SecondaryEntity target)
        {
            Trace.Assert(target?.TopLevelClosure != null);
            if (target?.TopLevelClosure != null)
            {
                return TopLevelClosure.Value.Propagate(target.TopLevelClosure.Value);
            }

            return false;
        }

        public virtual bool PropagateDown(SecondaryEntity target)
        {
            Trace.Assert(target?.BottomLevelClosure != null);
            if (target?.BottomLevelClosure != null)
            {
                return BottomLevelClosure.Value.Propagate(target.BottomLevelClosure.Value);
            }

            return false;
        }

        public virtual bool PropagateForward(SecondaryEntity target)
        {
            //Trace.Assert(target?.TopLevelClosure != null);
            if (target?.TopLevelClosure != null)
            {
                var shouldContinue = TopLevelClosure.Value.Propagate(target.TopLevelClosure.Value);
                shouldContinue = shouldContinue && BottomLevelClosure.Value.Propagate(target.BottomLevelClosure.Value);
                return shouldContinue;
            }

            return false;
        }

        private void OnNewTopEntityAdded(PrimaryEntity entity)
        {
            if (!BottomLevelClosure.Value.Collected.Contains(entity))
            {
                if (mySubscriptions.IsValueCreated)
                {
                    mySubscriptions.Value.Fire(entity);
                }
            }
        }

        private void OnNewBottomEntityAdded(PrimaryEntity entity)
        {
            if (!TopLevelClosure.Value.Collected.Contains(entity))
            {
                if (mySubscriptions.IsValueCreated)
                {
                    mySubscriptions.Value.Fire(entity);
                }
            }
        }

        public sealed class MySubscribersManager
        {
            public event Action<PrimaryEntity> OnNewPrimaryAdded;

            public void Fire(PrimaryEntity entity)
            {
                OnNewPrimaryAdded?.Invoke(entity);
            }
        }
    }
}
