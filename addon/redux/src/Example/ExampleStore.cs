namespace Redux.Example;


public abstract record ExampleStoreAction : StoreAction {
    public abstract ExampleState Reducer(ExampleState state);
}

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
}
