using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Statements;
using Cofra.AbstractIL.Internal.Types;
using Cofra.AbstractIL.Internal.Types.Secondaries;
using JetBrains.Annotations;

namespace Cofra.Core.Analyzes.SourceFilterSink
{
    using Node = Int32;

    public struct StackSymbol
    {
        [NotNull] public readonly OperationEdge<Node> CallSite;

        public readonly ResolvedLocalVariable Owner;

        public StackSymbol(OperationEdge<int> callSite, ResolvedInvocationStatement<Node> invocation)
        {
            CallSite = callSite;
            Owner = null;

            if (invocation == null)
            {
                return;
            }

            var owner = invocation.TargetEntity;
            if (invocation.IsConstructor)
            {
                var index = new ParameterIndex(-1);
                Owner = invocation.ReturnedValues[index] as ResolvedLocalVariable;
            }
            else if (owner is ResolvedLocalVariable localVariable)
            {
                Owner = localVariable;
            }
            else if (owner is SecondaryEntity justSecondary)
            {
                Owner = Utils.FindClosestLocalOwner(justSecondary);
            }
        }

        public override bool Equals(object other)
        {
            return other is StackSymbol stackSymbol &&
                   CallSite.Equals(stackSymbol.CallSite) &&
                   (Owner?.Equals(stackSymbol.Owner) ?? stackSymbol.Owner == null);
        }

        public bool Equals(StackSymbol other)
        {
            return CallSite.Equals(other.CallSite) &&
                   (Owner?.Equals(other.Owner) ?? other.Owner == null);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (CallSite.GetHashCode() * 397) ^ (Owner != null ? Owner.GetHashCode() : 0);
            }
        }
    }
}
