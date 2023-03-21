

public partial class ExampleWired : Control {

    [WireToState(nameof(ExampleState.HeaderText), "%HeaderText", nameof(Label.Text))]
    public string HeaderText {get; set;}

    public override void _Ready()
    {
        ExampleStateStore.Instance.ConnectWiredAttributes(this);
    }

}