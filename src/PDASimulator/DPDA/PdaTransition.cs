namespace PDASimulator.DPDA
{
    public enum PdaAction
    {
        Push,
        Pop,
        Skip
    }

    public abstract class PdaTransition<TState>
    {
        public readonly TState NextState;

        public abstract PdaAction ActionType {get;}

        public PdaTransition(TState nextState)
        {
            NextState = nextState;
        }
    }

    public class PushTransition<TState, TStackSymbol> : PdaTransition<TState>
    {
        public readonly TStackSymbol Pushed;

        public override PdaAction ActionType => PdaAction.Push;

        public PushTransition(TState nextState, TStackSymbol pushed) : base(nextState)
        {
            Pushed = pushed;
        }
    }

    public class PopTransition<TState> : PdaTransition<TState>
    {
        public override PdaAction ActionType => PdaAction.Pop;

        public PopTransition(TState nextState) : base(nextState)
        {
        }
    }

    public class SkipTransition<TState> : PdaTransition<TState>
    {
        public override PdaAction ActionType => PdaAction.Skip;
        public readonly bool ChangeContext;

        public SkipTransition(TState nextState) : base(nextState)
        {
            ChangeContext = true;
        }

        public SkipTransition(TState nextState, bool changeContext) : base(nextState)
        {
            ChangeContext = changeContext;
        }
    }
}