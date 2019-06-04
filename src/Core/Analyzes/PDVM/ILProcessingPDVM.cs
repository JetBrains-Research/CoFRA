using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Internal.ControlStructures;
using Cofra.AbstractIL.Internal.Types;
using Cofra.AbstractIL.Internal.Types.Primaries;
using Cofra.AbstractIL.Internal.Types.Secondaries;
using PDASimulator.DataStructures.GSS;
using PDASimulator.Payloads;
using PDASimulator.PDVM;
using PDASimulator.SimulationCommon;

namespace Cofra.Core.Analyzes.PDVM
{
    using Node = Int32;

    public abstract class ILProcessingPDVM<TState, TStackSymbol> : 
        PDVM<TState, TStackSymbol, Node, OperationEdge<Node>, 
            PdaExtractingContext<GssNode<TStackSymbol, EmptyGssData>, OperationEdge<Node>>, 
            EmptyGssData>
    {
        protected GraphStructuredProgram<Node> Program;

        protected ILProcessingPDVM(GraphStructuredProgram<Node> program)
        {
            Program = program;
        }

        public event Action<PdaExtractingContext<GssNode<TStackSymbol, EmptyGssData>, OperationEdge<Node>>> OnFinishEvent;

        protected override void OnFinish(
            Head<TState, Node, 
                PdaExtractingContext<GssNode<TStackSymbol, EmptyGssData>, OperationEdge<Node>>, 
                GssNode<TStackSymbol, EmptyGssData>> head,
            OperationEdge<Node> sourceTransition)
        {
            OnFinishEvent?.Invoke(head.CurrentContext);
        }
        
        protected IEnumerable<IInvokable<Node>> CollectPossibleTargets(Entity reference, ResolvedMethodId targetId)
        {
            if (reference is ResolvedClassId staticClass)
            {
                IEnumerable<IInvokable<Node>> Iterator(ResolvedMethod<Node> method)
                {
                    if (method != null)
                    {
                        yield return method;
                    }
                }

                var resolvedMethod = Program.FindClassById(staticClass).FindMethodInFullHierarchy(targetId);
                return Iterator(resolvedMethod);
            }

            if (reference is SecondaryEntity secondary)
            {
                return secondary.CollectedPrimaries.Select(
                    type =>
                    {
                        if (type is ResolvedClassId classId)
                        {
                            var target = Program 
                                .FindClassById(classId)
                                .FindMethodInFullHierarchy(targetId);

                            return (IInvokable<Node>) target;
                        }

                        if (type is IInvokable<Node> invokable)
                        {
                            return invokable;
                        }

                        return null;
                    }).Where(target => target != null);
            }

            return Enumerable.Empty<IInvokable<Node>>();
        }
    }
}
