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
}
