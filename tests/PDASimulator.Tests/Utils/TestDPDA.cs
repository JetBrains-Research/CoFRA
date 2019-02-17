using System;
using System.Collections.Generic;
using System.Text;
using PDASimulator.DPDA;
using QuickGraph;

namespace PDASimulator.Tests.Utils
{
    class TestDPDA : IDPDA<int, int, int, TaggedEdge<int, string>>
    {
        private readonly Dictionary<(int state, int stackData, string tag), PdaTransition<int>> myTransitions;
        private readonly HashSet<(int state, int stackData)> myFinals;

        private readonly Func<int, int> myStartTransitionsProvider;
        private readonly Func<int> myInitialStackDataProvider;

        public TestDPDA(Func<int, int> startTransitionsProvider, Func<int> initialStackDataProvider)
        {
            myTransitions = new Dictionary<(int state, int stackData, string tag), PdaTransition<int>>();
            myFinals = new HashSet<(int state, int stackData)>();
            myStartTransitionsProvider = startTransitionsProvider;
            myInitialStackDataProvider = initialStackDataProvider;
        }

        public void AddTransition((int state, int stackData, string tag) condition, PdaTransition<int> transition)
        {
            myTransitions.Add(condition, transition);
        }

        public void AddTransition(
            (int state, IEnumerable<int> stackData, string tag) condition,
            PdaTransition<int> transition)
        {
            foreach (var stackSymbol in condition.stackData)
            {
                AddTransition((condition.state, stackSymbol, condition.tag), transition);
            }
        }

        public void AddFinal((int state, int stackData) condition)
        {
            myFinals.Add(condition);
        }

        public PdaTransition<int> Step(int state, int stackData, TaggedEdge<int, string> sourceTransition)
        {
            bool exists = myTransitions.TryGetValue((state, stackData, sourceTransition.Tag), out var transition);

            if (exists)
            {
                return transition;
            }

            return null;
        }

        public bool IsFinal(int state, int stackTop)
        {
            return myFinals.Contains((state, stackTop));
        }

        public int GetStartState(int startPosition)
        {
            return myStartTransitionsProvider(startPosition);
        }

        public int GetInitialStackData()
        {
            return myInitialStackDataProvider();
        }
    }
}
