namespace Redux.Example;

public partial class ExampleWired : Control
{
    [WireProperty(nameof(ExampleState.HeaderText), "%HeaderText", nameof(Label.Text))]
    public string HeaderText { get; set; }

    public override void _Ready()
    {
        this.ConnectAttributes(ExampleStore.Instance);

        GD.Print(ExampleStore.GetCurrentState().HeaderText);
        ExampleStore.Instance.Dispatch(new ExampleState.UpdateHeaderText(){newText = "Newer Text"});
        GD.Print(ExampleStore.GetCurrentState().HeaderText);

    }
}