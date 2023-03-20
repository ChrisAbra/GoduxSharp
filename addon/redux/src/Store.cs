namespace Redux;

public abstract record StoreAction  {
}

public abstract partial class Store : Node 
{
    public State CurrentState { get; protected set; }
    //public Dictionary<Type,Reducer> Reducers = new();
    protected readonly List<State> historicStates = new();

    public virtual void Dispatch(StoreAction action)
    {
        historicStates.Add(CurrentState);
        var reducerMethod = action.GetType().GetMethod("Reducer");
        var newState = (State)reducerMethod?.Invoke(action,new object[]{CurrentState}) ?? CurrentState;
        if(newState == CurrentState){
            return;
        }
        CurrentState = newState;
    }
}
