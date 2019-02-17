using System;
using System.Collections.Generic;
using System.Text;

namespace PDASimulator.DataStructures.SPPF
{
    public class Sppf<TExtension, TTransition>
    {
        private Dictionary<SppfNodeKey<TExtension>, IntermediateNode<TExtension>> myIntermediates;
        private Dictionary<SppfNodeKey<TExtension>, CompleteNode<TExtension>> myComplete;
        private Dictionary<(SppfNodeKey<TExtension>, TTransition), TerminalNode<TExtension, TTransition>> myTerminals;

        public Sppf()
        {
            myIntermediates = new Dictionary<SppfNodeKey<TExtension>, IntermediateNode<TExtension>>();
            myComplete = new Dictionary<SppfNodeKey<TExtension>, CompleteNode<TExtension>>();
            myTerminals = new Dictionary<(SppfNodeKey<TExtension>, TTransition), TerminalNode<TExtension, TTransition>>();
        }

        public IntermediateNode<TExtension> GetOrCreateIntermediateNode(SppfNodeKey<TExtension> key)
        {
            var exists = myIntermediates.TryGetValue(key, out var node);

            if (!exists)
            {
                node = new IntermediateNode<TExtension>(key);
                myIntermediates.Add(key, node);
            }

            return node;
        }

        public CompleteNode<TExtension> GetOrCreateCompleteNode(SppfNodeKey<TExtension> key)
        {
            var exists = myComplete.TryGetValue(key, out var node);

            if (!exists)
            {
                node = new CompleteNode<TExtension>(key);
                myComplete.Add(key, node);
            }

            return node;
        }

        public TerminalNode<TExtension, TTransition> GetOrCreateTerminalNode(SppfNodeKey<TExtension> key, TTransition token)
        {
            var fullKey = (key, token);
            var exists = myTerminals.TryGetValue(fullKey, out var node);

            if (!exists)
            {
                node = new TerminalNode<TExtension, TTransition>(key, token);
                myTerminals.Add(fullKey, node);
            }

            return node;
        }
    }
}
