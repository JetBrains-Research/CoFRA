using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Internal.Types;
using Cofra.AbstractIL.Internal.Types.Markers;
using Cofra.AbstractIL.Internal.Types.Secondaries;

namespace Cofra.Core.Analyzes.SourceFilterSink
{
    public static class Utils
    {
        public static ResolvedLocalVariable FindClosestLocalOwner(SecondaryEntity entity)
        {
            if (entity is ResolvedLocalVariable justLocalVariable)
            {
                return justLocalVariable;
            }

            if (entity is ResolvedObjectField field)
            {
                return FindClosestLocalOwner(field.OwningObject);
            }

            return null;
        }

        public static bool IsTaintedSource(Entity source)
        {
            return source is ResolvedClassField classField &&
                   classField.MarkedWith(TaintMarker.Instance) ||
                   source is ResolvedObjectField objectField &&
                   objectField.HasSpecificPrimary(TaintMarker.Instance);
        }
    }
}
