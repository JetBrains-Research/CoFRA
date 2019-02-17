using System;
using System.Collections.Generic;
using System.Text;

namespace PDASimulator.DataStructures.SPPF
{
    public sealed class TerminalNode<TExtension, TTransition> : NonPackedNode<TExtension>
    {
        public readonly TTransition Token;

        public TerminalNode(SppfNodeKey<TExtension> key, TTransition token) : base(key)
        {
            Token = token;
        }

        public override bool Equals(object obj)
        {
            return obj is TerminalNode<TExtension, TTransition> node &&
                   EqualityComparer<TTransition>.Default.Equals(Token, node.Token);
        }

        public override int GetHashCode()
        {
            return -524128606 + EqualityComparer<TTransition>.Default.GetHashCode(Token);
        }
    }
}
