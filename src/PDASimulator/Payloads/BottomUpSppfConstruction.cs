using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Permissions;
using System.Text;
using PDASimulator.DataStructures;
using PDASimulator.DataStructures.GSS;
using PDASimulator.DataStructures.SPPF;
using PDASimulator.DPDA;
using PDASimulator.DPDA.Simulation;
using PDASimulator.SimulationCommon;

namespace PDASimulator.Payloads
{
    using Extension = BottomUpSppfConstructingContextExtension;

    public sealed class BottomUpSppfConstructingContextExtension
    {
    }

    public sealed class BottomUpSppfConstructingContext<TGssNode> : Context<TGssNode>
    {
        public readonly Extension Extension;

        public BottomUpSppfConstructingContext(TGssNode stackTop) : base(stackTop)
        {
            Extension = new Extension();
        }
    }

    internal sealed class LocalSppfNodesIndex<TKey, TNode>
    {
        private readonly Dictionary<TKey, ISet<TNode>> myIndex;

        public IEnumerable<TNode> Values => myIndex.Values.SelectMany(_ => _);

        public LocalSppfNodesIndex()
        {
            myIndex = new Dictionary<TKey, ISet<TNode>>();
        }

        public IEnumerable<TNode> Find(TKey key)
        {
            var exists = myIndex.TryGetValue(key, out var nodes);

            if (!exists)
            {
                return Enumerable.Empty<TNode>();
            }

            return nodes;
        }

        public void Put(TKey key, TNode node)
        {
            var exists = myIndex.TryGetValue(key, out var nodes);

            if (!exists)
            {
                nodes = new HashSet<TNode>();
                myIndex.Add(key, nodes);
            }

            nodes.Add(node);
        }

        public void UnionWith(LocalSppfNodesIndex<TKey, TNode> source)
        {
            foreach (var pair in source.myIndex)
            {
                var exists = myIndex.TryGetValue(pair.Key, out var nodes);

                if (!exists)
                {
                    nodes = new HashSet<TNode>();
                    myIndex.Add(pair.Key, nodes);
                }

                nodes.UnionWith(pair.Value);
            }
        }
    }

    public sealed class BottomUpSppfConstructingGssData
    {
        private readonly LocalSppfNodesIndex<Extension, IntermediateNode<Extension>> myIntermediatesByRight;
        private readonly LocalSppfNodesIndex<Extension, CompleteNode<Extension>> myCompleteByRight;
        private readonly LocalSppfNodesIndex<Extension, CompleteNode<Extension>> myCompleteByLeft;

        public IEnumerable<CompleteNode<Extension>> CompleteNodes => myCompleteByRight.Values;

        public BottomUpSppfConstructingGssData()
        {
            myIntermediatesByRight = new LocalSppfNodesIndex<Extension, IntermediateNode<Extension>>();
            myCompleteByRight = new LocalSppfNodesIndex<Extension, CompleteNode<Extension>>();
            myCompleteByLeft = new LocalSppfNodesIndex<Extension, CompleteNode<Extension>>();
        }

        public void MergeInto(BottomUpSppfConstructingGssData target)
        {
        }

        public void Shift<TTransition>(
            Sppf<Extension, TTransition> sppf,
            Extension left,
            Extension right,
            TTransition token,
            BottomUpSppfConstructingGssData source)
        {
            var terminalKey = new SppfNodeKey<Extension>(left, right);
            var terminal = sppf.GetOrCreateTerminalNode(terminalKey, token);

            //epsilon prefix
            var parent = sppf.GetOrCreateIntermediateNode(terminalKey);
            parent.AddChild(new PackedNode<Extension>(terminal, null));
            Put(parent);

            //complete prefix
            foreach (var complete in source.myCompleteByRight.Find(left))
            {
                var key = complete.Key.Concat(terminalKey);
                parent = sppf.GetOrCreateIntermediateNode(key);
                parent.AddChild(new PackedNode<Extension>(complete, terminal));
                Put(parent);
            }

            /*
            //complete suffix 
            foreach (var complete in myCompleteByLeft.Find(right))
            {
                var key = terminalKey.Concat(complete.Key);
                parent = sppf.GetOrCreateIntermediateNode(key);
                parent.AddChild(new PackedNode<Extension>(terminal, complete));
                Put(parent);
            }
            */
        }

        public void Reduce<TTransition>(
            Sppf<Extension, TTransition> sppf,
            Extension left,
            Extension right,
            TTransition token,
            BottomUpSppfConstructingGssData target)
        {
            var terminalKey = new SppfNodeKey<Extension>(left, right);
            var terminal = sppf.GetOrCreateTerminalNode(terminalKey, token);

            // Epsilon reduce
            foreach (var intermediate in myIntermediatesByRight.Find(left).ToArray())
            {
                var key = intermediate.Key.Concat(terminalKey);
                var parent = sppf.GetOrCreateCompleteNode(key);
                parent.AddChild(new PackedNode<Extension>(intermediate, terminal));

                target.Put(parent);

                /*
                foreach (var updated in target.myIntermediatesByRight.Find(parent.Key.LeftExtension))
                {
                    var newKey = updated.Key.Concat(parent.Key);
                    var newParent = sppf.GetOrCreateIntermediateNode(newKey);
                    newParent.AddChild(new PackedNode<Extension>(updated, parent));
                    target.Put(newParent);
                }
                */
            }

            // Complete reduce
            foreach (var complete in myCompleteByRight.Find(left).ToArray())
            {
                var key = complete.Key.Concat(terminalKey);
                var parent = sppf.GetOrCreateIntermediateNode(key);
                parent.AddChild(new PackedNode<Extension>(complete, terminal));

                foreach (var intermediate in myIntermediatesByRight.Find(key.LeftExtension).ToArray())
                {
                    var fullKey = intermediate.Key.Concat(parent.Key);
                    var fullNode = sppf.GetOrCreateCompleteNode(fullKey);
                    fullNode.AddChild(new PackedNode<Extension>(intermediate, parent));

                    target.Put(fullNode);
                }

                /*
                foreach (var updated in target.myIntermediatesByRight.Find(parent.Key.LeftExtension))
                {
                    var newKey = updated.Key.Concat(parent.Key);
                    var newParent = sppf.GetOrCreateIntermediateNode(newKey);
                    newParent.AddChild(new PackedNode<Extension>(updated, parent));
                    target.Put(newParent);
                }
                */
            }
        }

        public void Append<TTransition>(
            Sppf<Extension, TTransition> sppf,
            Extension left,
            Extension right,
            TTransition token)
        {
            var terminalKey = new SppfNodeKey<Extension>(left, right);
            var terminal = sppf.GetOrCreateTerminalNode(terminalKey, token);

            //epsilon prefix
            var parent = sppf.GetOrCreateCompleteNode(terminalKey);
            parent.AddChild(new PackedNode<Extension>(terminal, null));
            Put(parent);

            //complete prefix
            foreach (var complete in myCompleteByRight.Find(left))
            {
                var key = complete.Key.Concat(terminalKey);
                parent = sppf.GetOrCreateCompleteNode(key);
                parent.AddChild(new PackedNode<Extension>(complete, terminal));
                Put(parent);
            }
        }

        public void PassOver<TTransition>(
            Sppf<Extension, TTransition> sppf,
            Extension previousRight,
            Extension newRight,
            BottomUpSppfConstructingGssData target)
        {
            foreach (var complete in myCompleteByRight.Find(previousRight))
            {
                var key = new SppfNodeKey<Extension>(complete.Key.LeftExtension, newRight);
                var continuation = sppf.GetOrCreateCompleteNode(key);
                continuation.AddChild(new PackedNode<Extension>(complete, null));
                target.Put(continuation);
            }

            foreach (var intermediate in myIntermediatesByRight.Find(previousRight))
            {
                var key = new SppfNodeKey<Extension>(intermediate.Key.LeftExtension, newRight);
                var continuation = sppf.GetOrCreateIntermediateNode(key);
                continuation.AddChild(new PackedNode<Extension>(intermediate, null));
                target.Put(continuation);
            }
        }

        public void MergeInto<TTransition>(
            Sppf<Extension, TTransition> sppf,
            BottomUpSppfConstructingGssData target)
        {
            foreach (var leftComplete in myCompleteByRight.Values)
            {
                foreach (var rightComplete in target.myCompleteByLeft.Find(leftComplete.Key.RightExtension))
                {
                    var key = leftComplete.Key.Concat(rightComplete.Key);
                    var parent = sppf.GetOrCreateCompleteNode(key);
                    parent.AddChild(new PackedNode<Extension>(leftComplete, rightComplete));
                    //Put(parent);
                    target.Put(parent);
                }
            }

            target.myCompleteByLeft.UnionWith(myCompleteByLeft);
            target.myCompleteByRight.UnionWith(myCompleteByRight);
            target.myIntermediatesByRight.UnionWith(myIntermediatesByRight);
        }

        private void Put(IntermediateNode<Extension> node)
        {
            myIntermediatesByRight.Put(node.Key.RightExtension, node);
        }

        private void Put(CompleteNode<Extension> node)
        {
            myCompleteByRight.Put(node.Key.RightExtension, node);
            myCompleteByLeft.Put(node.Key.LeftExtension, node);
        }
    }

    public class BottomUpSppfConstructingContextProcessor<TState, TStackSymbol, TPosition, TTransition> 
        : IContextProcessor<TState, TStackSymbol, TPosition, TTransition, BottomUpSppfConstructingContext<GssNode<TStackSymbol, BottomUpSppfConstructingGssData>>, GssNode<TStackSymbol, BottomUpSppfConstructingGssData>>
    {
        private readonly Sppf<Extension, TTransition> mySppf;

        public BottomUpSppfConstructingContextProcessor(Sppf<Extension, TTransition> sppf)
        {
            mySppf = sppf;
        }

        public BottomUpSppfConstructingContext<GssNode<TStackSymbol, BottomUpSppfConstructingGssData>> 
            HeadToContext(Head<TState, TPosition, 
                BottomUpSppfConstructingContext<GssNode<TStackSymbol, BottomUpSppfConstructingGssData>>, 
                GssNode<TStackSymbol, BottomUpSppfConstructingGssData>> head)
        {
            return new BottomUpSppfConstructingContext<GssNode<TStackSymbol, BottomUpSppfConstructingGssData>>(head.StackTop);
        }

        public bool InheritContextsOnPush(
            BottomUpSppfConstructingContext<GssNode<TStackSymbol, BottomUpSppfConstructingGssData>> parent, 
            BottomUpSppfConstructingContext<GssNode<TStackSymbol, BottomUpSppfConstructingGssData>> child,
            TState nextState, TStackSymbol pushed, TTransition sourceTransition, bool memoized)
        {
            child.StackTop.UserData.Shift(mySppf, parent.Extension, child.Extension, sourceTransition, parent.StackTop.UserData);
            return true;
        }

        public bool InheritContextsOnSkip(
            BottomUpSppfConstructingContext<GssNode<TStackSymbol, BottomUpSppfConstructingGssData>> parent, 
            BottomUpSppfConstructingContext<GssNode<TStackSymbol, BottomUpSppfConstructingGssData>> child,
            TState nextState, TTransition sourceTransition, bool memoized)
        {
            parent.StackTop.UserData.Append(mySppf, parent.Extension, child.Extension, sourceTransition);
            return true;
        }

        public bool InheritContextsOnPop(
            BottomUpSppfConstructingContext<GssNode<TStackSymbol, BottomUpSppfConstructingGssData>> parent, 
            BottomUpSppfConstructingContext<GssNode<TStackSymbol, BottomUpSppfConstructingGssData>> child,
            TState nextState, TTransition sourceTransition, bool memoized)
        {
            parent.StackTop.UserData.Reduce(mySppf, parent.Extension, child.Extension, sourceTransition, child.StackTop.UserData);
            return true;
        }

        public void MergeContexts(
            BottomUpSppfConstructingContext<GssNode<TStackSymbol, BottomUpSppfConstructingGssData>> source, 
            BottomUpSppfConstructingContext<GssNode<TStackSymbol, BottomUpSppfConstructingGssData>> target)
        {
            if (ReferenceEquals(source, target))
            {
                return;
            }

            source.StackTop.UserData.PassOver(mySppf, source.Extension, target.Extension, target.StackTop.UserData);
        }

        public void MergeGssData(GssNode<TStackSymbol, BottomUpSppfConstructingGssData> source, GssNode<TStackSymbol, BottomUpSppfConstructingGssData> target)
        {
            if (ReferenceEquals(source.UserData, target.UserData))
            {
                return;
            }

            source.UserData.MergeInto(mySppf, target.UserData);
        }
    }
}
