using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PDASimulator.DataStructures;
using PDASimulator.DataStructures.GSS;
using PDASimulator.SimulationCommon;
using PDASimulator.Utils;

namespace PDASimulator.PDVM.Simulation
{
    public class PDVMSimulation<TState, TStackSymbol, TPosition, TTransition, TContext, TGssData> 
        where TContext : Context<GssNode<TStackSymbol, TGssData>> 
        where TGssData : new()
    {
        private readonly IContextProcessor<TState, TStackSymbol, TPosition, TTransition, TContext, GssNode<TStackSymbol, TGssData>> myContextProcessor;
        private readonly ICacheProvider<TState, TStackSymbol, TPosition, TContext, GssNode<TStackSymbol, TGssData>> myCacheProvider;
        private readonly ITransitionProvider<TPosition, TTransition> myTransitionProvider;
        private readonly PopHistory<TState, TTransition, TPosition, TContext, GssNode<TStackSymbol, TGssData>> myPopHistory;

        private PDVM<TState, TStackSymbol, TPosition, TTransition, TContext, TGssData> myPdvm;
        public PDVM<TState, TStackSymbol, TPosition, TTransition, TContext, TGssData> Pdvm => myPdvm;

        private readonly Dictionary<TStackSymbol, GssNode<TStackSymbol, TGssData>> myGssRoots;

        private readonly Queue<Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>>> myStarts;

        private MyPDVMState myState;

        public PDVMSimulation(
            IContextProcessor<TState, TStackSymbol, TPosition, TTransition, TContext, GssNode<TStackSymbol, TGssData>> contextProcessor,
            ICacheProvider<TState, TStackSymbol, TPosition, TContext, GssNode<TStackSymbol, TGssData>> cacheProvider,
            ITransitionProvider<TPosition, TTransition> transitionProvider,
            Func<PDVM<TState, TStackSymbol, TPosition, TTransition, TContext, TGssData>> pdvmProvider)
        {
            myContextProcessor = contextProcessor;
            myCacheProvider = cacheProvider;
            myTransitionProvider = transitionProvider;
            myPopHistory = new PopHistory<TState, TTransition, TPosition, TContext, GssNode<TStackSymbol, TGssData>>();

            myPdvm = pdvmProvider();

            myGssRoots = new Dictionary<TStackSymbol, GssNode<TStackSymbol, TGssData>>();

            myStarts = new Queue<Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>>>();
        }

        public TContext Load(TPosition start, TState initialState, TStackSymbol initialStackSymbol)
        {
            var gssRoot = myGssRoots.GetOrCreate(initialStackSymbol, 
                () => new GssNode<TStackSymbol, TGssData>(initialStackSymbol));

            var head = NewHead(initialState, gssRoot, start, null);

            myStarts.Enqueue(head);
            return head.CurrentContext;
        }

        public void ForceExecute<TSpecific>(
            Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>> head,
            TTransition transition,
            Action<TState, GssNode<TStackSymbol, TGssData>, TPosition, TTransition, TSpecific> program,
            TSpecific data)
        {
            var storedHead = myState.CurrentHead;
            var storedTransition = myState.CurrentTransition;
            var storedTargetPosition = myState.NextPosition;

            myState.SetCurrentHead(head);
            myState.SetCurrentTransition(transition, myTransitionProvider.Target(transition));

            program(head.State, head.StackTop, head.Position, transition, data);

            myState.SetCurrentHead(storedHead);
            myState.SetCurrentTransition(storedTransition, storedTargetPosition);
        }

        public void ForceExecute<TSpecific>(
            TState state,
            GssNode<TStackSymbol, TGssData> stack,
            TPosition position,
            TTransition transition,
            Action<TState, GssNode<TStackSymbol, TGssData>, TPosition, TTransition, TSpecific> program,
            TSpecific data)
        {
            var newHead =
                new Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>>(state, stack, position);

            if (myCacheProvider.Visited(position, state, stack.Symbol, out var context))
            {
                newHead.CurrentContext = context;
            }
            else if (myCacheProvider.VisitedOnPop(position, state, stack, out context))
            {
                newHead.CurrentContext = context;
            }
            else
            {
                newHead.CurrentContext = myContextProcessor.HeadToContext(newHead);
            }

            ForceExecute(newHead, transition, program, data);
        }

        public void Run()
        { 
            myState = new MyPDVMState(myContextProcessor, myCacheProvider, myPopHistory);
            myPdvm.UpdateState(myState);

            var processed = 0;
            while (myStarts.Count > 0)
            {
                var start = myStarts.Dequeue();

                myState.PushHead(start);

                if (processed % 1000 == 0)
                {
                    Console.Out.WriteLine(processed);
                }

                processed++;

                while (myState.HasNext())
                {
                    /*
                    foreach (var action in myState.DequeueAllActions())
                    {
                        action();
                    }
                    */

                    var head = myState.NextHead();
                    myState.SetCurrentHead(head);
                    myState.SetCurrentTransition(default(TTransition), head.Position);

                    myPdvm.Action(head.State, head.StackTop);

                    foreach (var transition in myTransitionProvider.Transitions(head.Position))
                    {
                        var target = myTransitionProvider.Target(transition);
                        myState.SetCurrentTransition(transition, target);

                        myPdvm.Step(head.State, head.StackTop, head.Position, transition);
                    }
                }
            }

            ExecuteAllQueuedActions();
        }

        public void ExecuteAllQueuedActions()
        {
            foreach (var action in myState.DequeueAllActions())
            {
                action();
            }
        }

        private Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>> NewHead(
            TState state, 
            GssNode<TStackSymbol, TGssData> stackTop, 
            TPosition position,
            TContext reuseContext)
        {
            var head = new Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>>(
                state, stackTop, position);

            head.CurrentContext = reuseContext ?? myContextProcessor.HeadToContext(head);

            return head;
        }

        private class MyPDVMState : PDVMState<TState, TStackSymbol, TPosition, TTransition, TContext, TGssData>
        {
            private readonly Stack<Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>>> myProcessing;
            private readonly Stack<Action> myQueued;

            private TTransition myCurrentTransition;
            private Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>> myCurrentHead;

            public override IContextProcessor<TState, TStackSymbol, TPosition, 
                TTransition, TContext, GssNode<TStackSymbol, TGssData>> ContextProcessor { get; }

            public override ICacheProvider<TState, TStackSymbol, TPosition, 
                TContext, GssNode<TStackSymbol, TGssData>> CacheProvider { get; }

            public override PopHistory<TState, TTransition, TPosition, TContext, GssNode<TStackSymbol, TGssData>> PopHistory { get; }

            public override TTransition CurrentTransition => myCurrentTransition;
            public override TPosition NextPosition { get; set; }

            public override Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>> CurrentHead =>
                myCurrentHead;

            public MyPDVMState(
                IContextProcessor<TState, TStackSymbol, TPosition, 
                    TTransition, TContext, GssNode<TStackSymbol, TGssData>> contextProcessor,
                ICacheProvider<TState, TStackSymbol, TPosition, 
                    TContext, GssNode<TStackSymbol, TGssData>> cacheProvider,
                PopHistory<TState, TTransition, TPosition, 
                    TContext, GssNode<TStackSymbol, TGssData>> popHistory)
            {
                ContextProcessor = contextProcessor;
                CacheProvider = cacheProvider;
                PopHistory = popHistory;

                myProcessing = new Stack<Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>>>();
                myQueued = new Stack<Action>();
            }

            public bool HasNext()
            {
                return myProcessing.Count > 0;
            }

            public Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>> NextHead()
            {
                if (myProcessing.Count == 0)
                {
                    return null;
                }

                return myProcessing.Pop();
            }

            public override void PushHead(Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>> head)
            {
                myProcessing.Push(head);
            }

            public void SetCurrentHead(Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>> head)
            {
                myCurrentHead = head;
            }

            public void SetCurrentTransition(TTransition transition, TPosition target)
            {
                myCurrentTransition = transition;
                NextPosition = target;
            }

            public void EnqueueForcedAction(Action action)
            {
                myQueued.Push(action);
            }

            public IEnumerable<Action> DequeueAllActions()
            {
                var list = myQueued.ToList();
                myQueued.Clear();

                return list;
            }
        }
    }
}
