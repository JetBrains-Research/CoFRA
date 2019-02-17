using System;
using System.Collections.Generic;
using System.Text;

namespace PDASimulator.DataStructures.SPPF
{
    public sealed class CompleteNode<TExtension> : ProducingNode<TExtension>
    {
        public CompleteNode(SppfNodeKey<TExtension> key)
            : base(key)
        {
        }

        public override bool Equals(object obj)
        {
            return obj is CompleteNode<TExtension> node &&
                   base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 324022166 + base.GetHashCode();
        }
    }
}
