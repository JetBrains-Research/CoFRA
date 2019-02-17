using System;
using System.Collections.Generic;
using System.Text;

namespace PDASimulator.DataStructures.SPPF
{
    public abstract class ProducingNode<TExtension> : NonPackedNode<TExtension>
    {
        private readonly HashSet<PackedNode<TExtension>> myChildren;

        public IEnumerable<PackedNode<TExtension>> Children => myChildren;

        public ProducingNode(SppfNodeKey<TExtension> key)
            : base(key)
        {
            myChildren = new HashSet<PackedNode<TExtension>>();
        }

        public void AddChild(PackedNode<TExtension> child)
        {
            myChildren.Add(child);
        }

        public override bool Equals(object obj)
        {
            return obj is ProducingNode<TExtension> node &&
                   base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 324022166 + base.GetHashCode();
        }
    }
}
