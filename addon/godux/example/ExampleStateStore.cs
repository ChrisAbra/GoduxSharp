using System.Collections.Immutable;
using Godot;

public partial class ExampleStateStore : Godux.StateStore<ExampleState>
{
    protected override string Path => "/root/AppState";

    public record ChangedHeaderText(string HeaderText) : Action;
    public record IncrementCounter() : Action;
    public record DecrementCounter() : Action;
    public record UndoUndoableString() : Action;
    public record RedoUndoableString() : Action;
    public record SetUndoableString(string NewValue) : Action;

    public ExampleStateStore()
    {
        CurrentState = new ExampleState();
    }

    public override void _Ready()
    {
        Instance = this.GetNode<ExampleStateStore>(Path);

        On(typeof(ChangedHeaderText), (state, action) =>
        {
            var headerUpdateAction = action as ChangedHeaderText;
            return state with { HeaderText = headerUpdateAction.HeaderText };
        });
        On(typeof(DecrementCounter), (state, _) =>
        {
            var counter = state.Counter - 1;
            return state with { Counter = counter };
        });
        On(typeof(IncrementCounter), (state, _) =>
        {
            var counter = state.Counter + 1;
            return state with { Counter = counter };
        });
        On(typeof(UndoUndoableString), (state, _) => state with { UndoableString = state.UndoableString.Undo() });
        On(typeof(RedoUndoableString), (state, _) => state with { UndoableString = state.UndoableString.Redo() });
        On(typeof(SetUndoableString), (state, action) =>
        {
            var setUndoableAction = action as SetUndoableString;
            return state with { UndoableString = state.UndoableString.Set(setUndoableAction.NewValue)};
        });
    }
}