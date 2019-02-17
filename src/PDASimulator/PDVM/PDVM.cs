using System.Linq;
using PDASimulator.DataStructures;
using PDASimulator.DataStructures.GSS;
using PDASimulator.SimulationCommon;

namespace PDASimulator.PDVM
{
    public abstract class PDVM<TState, TStackSymbol, TPosition, TTransition, TContext, TGssData> 
        where TGssData : new() 
        where TContext : Context<GssNode<TStackSymbol, TGssData>>
    {
        private PDVMState<TState, TStackSymbol, TPosition, TTransition, TContext, TGssData> myState;

        protected Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>> ExtractHead()
        {
            return myState.CurrentHead;
        }

        protected void Push(TState newState, TStackSymbol data, TPosition jump)
        {
            var oldTarget = myState.NextPosition;
            myState.NextPosition = jump;

            Push(newState, data);

            myState.NextPosition = oldTarget;
        }

        protected void Push(TState newState, TStackSymbol data)
        {
            var head = myState.CurrentHead;

            var newData = data;
            var newPosition = myState.NextPosition;

            var processed = myState.CacheProvider.Visited(newPosition, newState, newData, out var previousContext);

            if (processed)
            {
                var prevTop = previousContext.StackTop;
                prevTop.AddParent(head.StackTop);

                myState.ContextProcessor.InheritContextsOnPush(
                    head.CurrentContext, 
                    previousContext, 
                    newState,
                    newData, 
                    myState.CurrentTransition, 
                    true);

                foreach (var record in myState.PopHistory.GetHistoryForNode(prevTop))
                {
                    ProcessPopForNewTop(record.NextState, record.SourceTransition, record.TargetPosition, record.InheritFrom, head.StackTop);
                }
            }
            else
            {
                var newTop = head.StackTop.Push(newData);
                var newHead = NewHead(newState, newTop, newPosition, null);

                myState.CacheProvider.Visit(newPosition, newState, newData, newHead.CurrentContext);

                var toContinue = myState.ContextProcessor.InheritContextsOnPush(
                    head.CurrentContext, 
                    newHead.CurrentContext, 
                    newState,
                    newData,
                    myState.CurrentTransition,
                    false);

                if (toContinue)
                {
                    myState.PushHead(newHead);
                }
            }
        }

        private void ProcessSkipAction(TState newState, bool changeContext)
        {
            var head = myState.CurrentHead;

            var newData = head.StackTop.Symbol;
            var newTop = head.StackTop;
            var newPosition = myState.NextPosition;

            var processed = myState.CacheProvider.VisitedOnPop(newPosition, newState, newTop, out var previousContext);

            if (processed)
            {
                myState.ContextProcessor.InheritContextsOnSkip(
                    head.CurrentContext, 
                    previousContext, 
                    newState,
                    myState.CurrentTransition, 
                    true);
            }
            else
            {
                Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>> newHead;

                var toContinue = true;
                if (changeContext)
                {
                    newHead = NewHead(newState, newTop, newPosition, null);
                    myState.CacheProvider.VisitOnPop(newPosition, newState, newTop, newHead.CurrentContext);

                    toContinue = myState.ContextProcessor.InheritContextsOnSkip(
                        head.CurrentContext, 
                        newHead.CurrentContext, 
                        newState,
                        myState.CurrentTransition, 
                        false);
                }
                else
                {
                    newHead = NewHead(newState, newTop, newPosition, head.CurrentContext);
                    myState.CacheProvider.VisitOnPop(newPosition, newState, newTop, newHead.CurrentContext);
                }

                if (toContinue)
                {
                    myState.PushHead(newHead);
                }
            }
        }

        protected void Accept(TState newState, TPosition jump)
        {
            var oldTarget = myState.NextPosition;
            myState.NextPosition = jump;

            Accept(newState);

            myState.NextPosition = oldTarget;
        }

        protected void Accept(TState newState)
        {
            ProcessSkipAction(newState, true);
        }

        protected void Skip(TState newState, TPosition jump)
        {
            var oldTarget = myState.NextPosition;
            myState.NextPosition = jump;

            Skip(newState);

            myState.NextPosition = oldTarget;
        }

        protected void Skip(TState newState)
        {
            ProcessSkipAction(newState, false);
        }

        private void ProcessPopForNewTop(
            TState nextState,
            TTransition sourceTransition,
            TPosition newPosition,
            TContext inheritFrom,
            GssNode<TStackSymbol, TGssData> newTop)
        {
            var newState = nextState;

            var processed = myState.CacheProvider.Visited(newPosition, newState, newTop.Symbol, out var previousContext);

            if (processed)
            {
                myState.ContextProcessor.InheritContextsOnPop(
                    inheritFrom, 
                    previousContext, 
                    newState,
                    sourceTransition, 
                    true);
            }
            else
            {
                var newHead = NewHead(newState, newTop, newPosition, null);
                var toContinue = myState.ContextProcessor.InheritContextsOnPop(
                    inheritFrom, 
                    newHead.CurrentContext, 
                    newState,
                    sourceTransition, 
                    false);

                myState.CacheProvider.Visit(newPosition, newState, newTop.Symbol, newHead.CurrentContext);
                if (toContinue)
                {
                    myState.PushHead(newHead);
                }
            }
        }

        protected void Pop(TState newState, TPosition jump)
        {
            var oldTarget = myState.NextPosition;
            myState.NextPosition = jump;

            Pop(newState);

            myState.NextPosition = oldTarget;
        }

        protected void Pop(TState nextState)
        {
            var head = myState.CurrentHead;

            var currentTop = head.CurrentContext.StackTop;
            var record = new PopHistoryRecord<TState, TTransition, TPosition, TContext>(
                    nextState, myState.CurrentTransition, myState.NextPosition, head.CurrentContext);

            myState.PopHistory.AddToPopHistory(currentTop, record);

            foreach (var newTop in currentTop.Pop())
            {
                ProcessPopForNewTop(nextState, myState.CurrentTransition, myState.NextPosition, head.CurrentContext, newTop);
            }
        }

        protected void Finish()
        {
            OnFinish(myState.CurrentHead, myState.CurrentTransition);
        }

        public abstract void Action(
            TState state,
            GssNode<TStackSymbol, TGssData> stack);

        public abstract void Step(
            TState state, 
            GssNode<TStackSymbol, TGssData> stack,
            TPosition position,
            TTransition currentTransition);

        protected abstract void OnFinish(
            Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>> head,
            TTransition sourceTransition);

        internal void UpdateState(PDVMState<TState, TStackSymbol, TPosition, TTransition, TContext, TGssData> state)
        {
            myState = state;
        }

        private Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>> NewHead(
            TState state, 
            GssNode<TStackSymbol, TGssData> stackTop, 
            TPosition position,
            TContext reuseContext)
        {
            var head = new Head<TState, TPosition, TContext, GssNode<TStackSymbol, TGssData>>(
                state, stackTop, position);

            head.CurrentContext = reuseContext ?? myState.ContextProcessor.HeadToContext(head);

            return head;
        }
    }
}
