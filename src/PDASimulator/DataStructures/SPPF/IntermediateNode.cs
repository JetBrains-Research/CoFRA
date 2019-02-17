using System;
using System.Collections.Generic;
using System.Text;

namespace PDASimulator.DataStructures.SPPF
{
    public sealed class IntermediateNode<TExtension> : ProducingNode<TExtension>
    {
        public IntermediateNode(SppfNodeKey<TExtension> key)
            : base(key)
        {
        }

        public override bool Equals(object obj)
        {
            return obj is IntermediateNode<TExtension> node &&
                   base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 624022166 + base.GetHashCode();
        }
    }
}
