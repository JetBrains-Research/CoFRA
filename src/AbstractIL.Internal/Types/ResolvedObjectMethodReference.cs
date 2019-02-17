using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Cofra.AbstractIL.Internal.Types
{
    [DataContract]
    public sealed class ResolvedObjectMethodReference<TNode> : SecondaryEntity
    {
        [DataMember]
        public readonly SecondaryEntity OwningObject;

        [DataMember]
        public readonly ResolvedMethodId MethodId;

        public Func<ResolvedClassId, ResolvedMethodId, ResolvedMethod<TNode>> FindClassMethod { get; set; } 

        public ResolvedObjectMethodReference(SecondaryEntity owningObject, ResolvedMethodId methodId)
        {
            OwningObject = owningObject;
            MethodId = methodId;
        }

        public override void ResetPropagatedTypes()
        {
            base.ResetPropagatedTypes();

            OwningObject.Subscriptions.OnNewPrimaryAdded += OnNewOwnerInstanceAdded;
        }

        public override bool PropagateUp(SecondaryEntity target)
        {
            return PropagateForward(target);
        }

        public override bool PropagateDown(SecondaryEntity target)
        {
            return PropagateForward(target);
        }

        private void OnNewOwnerInstanceAdded(PrimaryEntity entity)
        {
            var classId = entity as ResolvedClassId;
            //Trace.Assert(classId != null);

            var method = FindClassMethod(classId, MethodId);
            if (method != null)
            {
                AddPrimary(method);
            }
        }
    }
}
