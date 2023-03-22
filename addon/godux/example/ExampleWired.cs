using Godot;
using Godux;
using AppState = ExampleStateStore;
public partial class ExampleWired : Control
{

    [WireToState(nameof(ExampleState.HeaderText), "%HeaderText", nameof(Label.Text))]
    public string HeaderText { get; set; }

    [WireToState(new string[]{
        nameof(ExampleState.CounterSubState),
        nameof(ExampleSubState.CounterString)
        }, "%Counter", nameof(Label.Text))]
    public string CounterStringValue { get; set; }

    public override void _Ready()
    {
        AppState.Instance.ConnectWiredAttributes(this);
        ChangeText();
    }

    public void ChangeText()
    {
        AppState.Instance.Dispatch(new AppState.ChangedHeaderText("Cooler HeaderText"));
    }

    public void IncrementCounter()
    {
        AppState.Instance.Dispatch(new AppState.IncrementCounter());
    }
    public void DecrementCounter()
    {
        AppState.Instance.Dispatch(new AppState.DecrementCounter());
    }

}