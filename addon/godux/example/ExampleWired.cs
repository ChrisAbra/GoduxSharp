using Godot;
using AppState = Godux.Example.ExampleStateStore;

namespace Godux.Example;
public partial class ExampleWired : Control
{

    [WireToState(nameof(ExampleState.UndoableStringPresent), "%HeaderText", nameof(Label.Text))]
    public string HeaderText { get; set; }

    [WireToState(new string[] { nameof(ExampleState.CounterString) }, "%Counter", nameof(Label.Text))]
    public string CounterStringValue
    {
        get => _counterStringValue;
        set
        {
            _counterStringValue = value;
            CounterSet(value);
        }
    }
    private string _counterStringValue;

    public override void _Ready()
    {
        AppState.Instance.ConnectWiredAttributes(this);
        AppState.Instance.Dispatch(new AppState.SubStateStringUpdater("new Substate value"));
    }

    public void CounterSet(string value)
    {
        GD.Print("Counter Set with new value: ", value);
    }

    public void OnSetText()
    {
        var newHeaderText = GetNode<TextEdit>("%TitleTextEditor").Text;

        AppState.Instance.Dispatch(new AppState.SetUndoableString(newHeaderText));
        GetNode<TextEdit>("%TitleTextEditor").Text = "";

    }
    public void OnUndoTitle()
    {
        AppState.Instance.Dispatch(new AppState.UndoUndoableString());
    }

    public void OnRedoTitle()
    {
        AppState.Instance.Dispatch(new AppState.RedoUndoableString());
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