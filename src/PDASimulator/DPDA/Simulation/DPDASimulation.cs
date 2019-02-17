using System;
using System.Collections.Generic;
using PDASimulator.DataStructures;
using PDASimulator.DataStructures.GSS;
using PDASimulator.SimulationCommon;

namespace PDASimulator.DPDA.Simulation
{
    public class DPDASimulation<TState, TStackSymbol, TPosition, TTransition, TContext, TGssData>
        where TContext : Context<GssNode<TStackSymbol, TGssData>>
        where TGssData : new()
        where TStackSymbol : new()
    {
        private readonly IDPDA<TState, TStackSymbol, TPosition, TTransition> myPda;
        private readonly IContextProcessor<TState, TStackSymbol, TPosition, TTransition, TContext, GssNode<TStackSymbol, TGssData>> myContextProcessor;
        private readonly ICacheProvider<TState, TStackSymbol, TPosition, TContext, GssNode<TStackSymbol, TGssData>> myCacheProvider;
        private readonly ITransitionProvider<TPosition, TTransition> myTransitionProvider;

        private readonly PopHistory<TState, TTransition, TPosition, TContext, GssNode<TStackSymbol, TGssData>> myPopHistory;

        private readonly Stack<MyHead> myProcessing;
        private readonly Queue<MyHead> myStarts;

        private readonly GssNode<TStackSymbol, TGssData> myStackRoot;

        public event Action<Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>>> OnFinalState;

        public DPDASimulation(
            IDPDA<TState, TStackSymbol, TPosition, TTransition> pda, 
            IContextProcessor<TState, TStackSymbol, TPosition, TTransition, TContext, GssNode<TStackSymbol, TGssData>> contextProcessor,
            ICacheProvider<TState, TStackSymbol, TPosition, TContext, GssNode<TStackSymbol, TGssData>> cacheProvider,
            ITransitionProvider<TPosition, TTransition> transitionProvider)
        {
            myPda = pda;
            myContextProcessor = contextProcessor;
            myCacheProvider = cacheProvider;
            myTransitionProvider = transitionProvider;

            myProcessing = new Stack<MyHead>();
            myStarts = new Queue<MyHead>();

            myStackRoot = new GssNode<TStackSymbol, TGssData>(pda.GetInitialStackData());

            myPopHistory = new PopHistory<TState, TTransition, TPosition, TContext, GssNode<TStackSymbol, TGssData>>();
        }

        private MyHead NewHead(
            TState state, 
            GssNode<TStackSymbol, TGssData> stackTop, 
            TPosition position,
            TContext reuseContext)
        {
            var head = new MyHead(state, stackTop, position);

            if (reuseContext != null)
            {
                head.CurrentContext = reuseContext;
            }
            else
            {
                head.CurrentContext = myContextProcessor.HeadToContext(head);
            }

            return head;
        }

        public TContext Load(TPosition start)
        {
            var state = myPda.GetStartState(start);
            var head = NewHead(state, myStackRoot, start, null);
            myStarts.Enqueue(head);
            myCacheProvider.Visit(start, state, myStackRoot.Symbol, head.CurrentContext);

            return head.CurrentContext;
        }

        private void ProcessPush(MyHead head,
            PushTransition<TState, TStackSymbol> pdaTransition, TTransition sourceTransition)
        {
            var newData = pdaTransition.Pushed;
            var newState = pdaTransition.NextState;
            var newPosition = myTransitionProvider.Target(sourceTransition);

            var processed = myCacheProvider.Visited(newPosition, newState, newData, out var previousContext);

            if (processed)
            {
                var prevTop = previousContext.StackTop;
                prevTop.AddParent(head.StackTop);

                myContextProcessor.InheritContextsOnPush(
                    head.CurrentContext, 
                    previousContext, 
                    pdaTransition.NextState, 
                    pdaTransition.Pushed, 
                    sourceTransition, 
                    true);

                foreach (var record in myPopHistory.GetHistoryForNode(prevTop))
                {
                    ProcessPopForNewTop(record.NextState, record.SourceTransition, record.InheritFrom, head.StackTop);
                }
            }
            else
            {
                var newTop = head.StackTop.Push(newData);
                var newHead = NewHead(newState, newTop, newPosition, null);

                myContextProcessor.InheritContextsOnPush(
                    head.CurrentContext, 
                    newHead.CurrentContext, 
                    pdaTransition.NextState,
                    pdaTransition.Pushed,
                    sourceTransition, 
                    false);

                myCacheProvider.Visit(newPosition, newState, newData, newHead.CurrentContext);
                myProcessing.Push(newHead);
            }
        }

        private void ProcessSkip(MyHead head,
            SkipTransition<TState> pdaTransition, TTransition sourceTransition)
        {
            var newData = head.StackTop.Symbol;
            var newState = pdaTransition.NextState;
            var newTop = head.StackTop;
            var newPosition = myTransitionProvider.Target(sourceTransition);

            var processed = myCacheProvider.VisitedOnPop(newPosition, newState, newTop, out var previousContext);

            if (processed)
            {
                myContextProcessor.InheritContextsOnSkip(
                    head.CurrentContext, 
                    previousContext, 
                    pdaTransition.NextState,
                    sourceTransition, 
                    true);
            }
            else
            {
                MyHead newHead;

                if (pdaTransition.ChangeContext)
                {
                    newHead = NewHead(newState, newTop, newPosition, null);
                    myContextProcessor.InheritContextsOnSkip(
                        head.CurrentContext, 
                        newHead.CurrentContext, 
                        pdaTransition.NextState,
                        sourceTransition, 
                        false);
                }
                else
                {
                    newHead = NewHead(newState, newTop, newPosition, head.CurrentContext);
                }

                myCacheProvider.VisitOnPop(newPosition, newState, newTop, newHead.CurrentContext);
                myProcessing.Push(newHead);
            }
        }

        private void ProcessPopForNewTop(
            TState nextState,
            TTransition sourceTransition,
            TContext inheritFrom,
            GssNode<TStackSymbol, TGssData> newTop)
        {
            var newPosition = myTransitionProvider.Target(sourceTransition);
            var newState = nextState;

            var processed = myCacheProvider.VisitedOnPop(newPosition, newState, newTop, out var previousContext);

            if (processed)
            {
                myContextProcessor.InheritContextsOnPop(
                    inheritFrom, 
                    previousContext, 
                    newState,
                    sourceTransition, 
                    true);
            }
            else
            {
                var newHead = NewHead(newState, newTop, newPosition, null);
                myContextProcessor.InheritContextsOnPop(
                    inheritFrom, 
                    newHead.CurrentContext, 
                    newState,
                    sourceTransition, 
                    false);

                myCacheProvider.VisitOnPop(newPosition, newState, newTop, newHead.CurrentContext);
                myProcessing.Push(newHead);
            }
        }

        private void ProcessPop(MyHead head,
            PopTransition<TState> pdaTransition, TTransition sourceTransition)
        {
            var record = new PopHistoryRecord<TState, TTransition, TPosition, TContext>(
                    pdaTransition.NextState, sourceTransition, myTransitionProvider.Target(sourceTransition), head.CurrentContext);

            myPopHistory.AddToPopHistory(head.StackTop, record);

            foreach (var newTop in head.StackTop.Pop())
            {
                ProcessPopForNewTop(pdaTransition.NextState, sourceTransition, head.CurrentContext, newTop);
            }
        }

        private bool Step()
        {
            var head = myProcessing.Pop();
            if (myPda.IsFinal(head.State, head.StackTop.Symbol))
            {
                OnFinalState?.Invoke(head);
            }

            var sourceTransitions = myTransitionProvider.Transitions(head.Position);
            
            foreach (var sourceTransition in sourceTransitions)
            {
                var pdaTransition = myPda.Step(head.State, head.StackTop.Symbol, sourceTransition);

                if (pdaTransition != null)
                {
                    switch (pdaTransition.ActionType)
                    {
                        case PdaAction.Skip:
                            ProcessSkip(head, (SkipTransition<TState>) pdaTransition, sourceTransition);
                            break;
                        case PdaAction.Push:
                            ProcessPush(head, (PushTransition<TState, TStackSymbol>) pdaTransition, sourceTransition);
                            break;
                        case PdaAction.Pop:
                            ProcessPop(head, (PopTransition<TState>) pdaTransition, sourceTransition);
                            break;
                    }
                }
            }

            return myProcessing.Count > 0;
        }
        
        public void Run()
        {
            while (myStarts.Count > 0)
            {
                Console.WriteLine(myStarts.Count);

                var currentStart = myStarts.Dequeue();
                myProcessing.Push(currentStart);
                while (Step()) {}
            }
        }

        private sealed class MyHead : Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>>
        {
            public MyHead(TState state, GssNode<TStackSymbol, TGssData> stackTop, TPosition position) : base(state, stackTop, position)
            {
            }
        }
    }
}
