using System;
using System.Collections.Generic;
using System.Text;

namespace PDASimulator.DataStructures.SPPF
{
    public sealed class SppfNodeKey<TExtension>
    {
        public readonly TExtension LeftExtension;
        public readonly TExtension RightExtension;

        public SppfNodeKey(TExtension leftExtension, TExtension rightExtension)
        {
            LeftExtension = leftExtension;
            RightExtension = rightExtension;
        }

        public SppfNodeKey<TExtension> Concat(
            SppfNodeKey<TExtension> right)
        {
            return new SppfNodeKey<TExtension>(LeftExtension, right.RightExtension);
        }

        public override bool Equals(object obj)
        {
            return obj is SppfNodeKey<TExtension> key &&
                   EqualityComparer<TExtension>.Default.Equals(LeftExtension, key.LeftExtension) &&
                   EqualityComparer<TExtension>.Default.Equals(RightExtension, key.RightExtension);
        }

        public override int GetHashCode()
        {
            var hashCode = 407658399;
            hashCode = hashCode * -1521134295 + EqualityComparer<TExtension>.Default.GetHashCode(LeftExtension);
            hashCode = hashCode * -1521134295 + EqualityComparer<TExtension>.Default.GetHashCode(RightExtension);
            return hashCode;
        }
    }
}
