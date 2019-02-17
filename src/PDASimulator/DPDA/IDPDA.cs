namespace PDASimulator.DPDA
{
    public interface IDPDA<TState, TStackSymbol, TPosition, TTransition>
    {
        PdaTransition<TState> Step(TState state, TStackSymbol stackData, TTransition sourceTransition);
        bool IsFinal(TState state, TStackSymbol stackTop);

        TState GetStartState(TPosition startPosition);
        TStackSymbol GetInitialStackData();
    }
}