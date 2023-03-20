namespace Redux.Example;


public partial class ExampleStore : Store
{
    private const string path = "/root/AppStateStore";
    public static ExampleStore Instance {get;private set;}
    public static ExampleState GetCurrentState()
    { 
        return (ExampleState)Instance.CurrentState;
    }

    public ExampleStore()
    {
        this.CurrentState = new ExampleState();
    }
    public override void _Ready(){
        Instance = this.GetNode<ExampleStore>(path);
    }
    public override void Dispatch(StoreAction action)
    {
        historicStates.Add(CurrentState);
        var reducerMethod = action.GetType().GetMethod("Reducer");
        var newState = (ExampleState)reducerMethod?.Invoke(action,new object[]{CurrentState}) ?? CurrentState;
        if(newState == CurrentState){
            return;
        }
        CurrentState = newState;
    }

}
