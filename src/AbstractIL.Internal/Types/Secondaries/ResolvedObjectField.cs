using System;
using System.Runtime.Serialization;
using Cofra.AbstractIL.Internal.Types.Primaries;

namespace Cofra.AbstractIL.Internal.Types.Secondaries
{
    [DataContract]
    public sealed class ResolvedObjectField : SecondaryEntity
    {
        [DataMember]
        public readonly SecondaryEntity OwningObject;

        [DataMember]
        public readonly int FieldId;

        public Func<ResolvedClassId, int, ResolvedClassField> FindClassField { get; set; } 

        public ResolvedObjectField(SecondaryEntity owningObject, int variableId)
        {
            OwningObject = owningObject;
            FieldId = variableId;
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

            //TODO: Trace.Assert(classId != null);
            if (classId == null)
            {
                return;
            }

            var field = FindClassField(classId, FieldId);
            field.PropagateForward(this);
            this.PropagateForward(field);
        }
    }
}
