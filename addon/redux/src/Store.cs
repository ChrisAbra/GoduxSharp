namespace Redux;
public abstract record StoreAction  {
    public abstract State Reducer(State state);
}

public abstract partial class Store : Node
{
    public State CurrentState { get; protected set; }
    //public Dictionary<Type,Reducer> Reducers = new();
    private readonly List<State> historicStates = new();

    public void Dispatch(StoreAction action)
    {
        historicStates.Add(CurrentState);
        var newState = action.Reducer(CurrentState);
        if(newState == CurrentState){
            return;
        }
        CurrentState = newState;
    }
}
