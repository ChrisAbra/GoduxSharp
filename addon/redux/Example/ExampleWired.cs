namespace Redux.Example;

public partial class ExampleWired : Control
{
    [WireProperty(nameof(ExampleState.HeaderText), "%HeaderText", nameof(Label.Text))]
    public string HeaderTextSomethingElse { get; set; }

    public override void _Ready()
    {
        this.ConnectAttributes(ExampleStore.Instance, (propertyInfo, oldValue,newValue) => {
            GD.Print(oldValue);
            GD.Print(newValue);
            this.SetPropertyValue(propertyInfo,newValue);
        });
    }

    public void ChangeText(){
        ExampleStore.Instance.Dispatch(new ExampleState.UpdateHeaderText(){newText = "Newer Text"});
    }
}