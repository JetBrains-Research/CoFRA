using System;
using System.Collections.Generic;
using System.Text;

namespace PDASimulator.DataStructures.SPPF
{
    public abstract class NonPackedNode<TExtension> : ISppfNode
    {
        public readonly SppfNodeKey<TExtension> Key;

        public NonPackedNode(SppfNodeKey<TExtension> key)
        {
            Key = key;
        }

        public override bool Equals(object obj)
        {
            return obj is NonPackedNode<TExtension> node &&
                   Key.Equals(node.Key);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
