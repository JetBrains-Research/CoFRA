using System;
using System.Collections.Generic;
using System.Text;
using PDASimulator.DataStructures.GSS;
using PDASimulator.Utils;

namespace PDASimulator.SimulationCommon
{
    public class PopHistory<TState, TTransition, TPosition, TContext, TGssNode> 
    {
        private readonly Dictionary<TGssNode, HashSet<PopHistoryRecord<TState, TTransition, TPosition, TContext>>> myRecords;

        public PopHistory()
        {
            myRecords = new Dictionary<TGssNode, HashSet<PopHistoryRecord<TState, TTransition, TPosition, TContext>>>();
        }

        public IEnumerable<PopHistoryRecord<TState, TTransition, TPosition, TContext>> GetHistoryForNode(TGssNode node)
        {
            return myRecords.GetOrCreate(node);
        }

        public void AddToPopHistory(TGssNode node, PopHistoryRecord<TState, TTransition, TPosition, TContext> record)
        {
            var history = myRecords.GetOrCreate(node);
            history.Add(record);
        }

        public void MergePopHistoryInto(TGssNode source, TGssNode target)
        {
            var sourceHistory = myRecords.GetOrCreate(source);
            var targetHistory = myRecords.GetOrCreate(target);

            targetHistory.UnionWith(sourceHistory);
        }
    }
}
