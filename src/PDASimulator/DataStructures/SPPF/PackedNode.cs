using System;
using System.Collections.Generic;
using System.Text;

namespace PDASimulator.DataStructures.SPPF
{
    public sealed class PackedNode<TExtension> : ISppfNode
    {
        public readonly NonPackedNode<TExtension> Left;
        public readonly NonPackedNode<TExtension> Right;

        public PackedNode(
            NonPackedNode<TExtension> left, 
            NonPackedNode<TExtension> right)
        {
            Left = left;
            Right = right;
        }

        public override bool Equals(object obj)
        {
            return obj is PackedNode<TExtension> node &&
                   EqualityComparer<NonPackedNode<TExtension>>.Default.Equals(Left, node.Left) &&
                   EqualityComparer<NonPackedNode<TExtension>>.Default.Equals(Right, node.Right);
        }

        public override int GetHashCode()
        {
            var hashCode = -1051820395;
            hashCode = hashCode * -1521134295 + EqualityComparer<NonPackedNode<TExtension>>.Default.GetHashCode(Left);
            hashCode = hashCode * -1521134295 + EqualityComparer<NonPackedNode<TExtension>>.Default.GetHashCode(Right);
            return hashCode;
        }
    }
}
