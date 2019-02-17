using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Internal.ControlStructures;

namespace Cofra.AbstractIL.Internal.Types
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
