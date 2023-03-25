using System.Collections.Immutable;
using Godot;

namespace Godux.Example;

public partial class ExampleStateStore : Godux.StateStore<ExampleState>
{
    //protected override string Path => "/root/AppState";

    public record ChangedHeaderText(string HeaderText) : Action;
    public record IncrementCounter : Action;
    public record DecrementCounter : Action;
    public record UndoUndoableString : Action;
    public record RedoUndoableString : Action;
    public record SetUndoableString(string NewValue) : Action;

    public ExampleStateStore()
    {
        CurrentState = new ExampleState();
    }

    public override void _Ready()
    {
        Instance = this.GetNode<ExampleStateStore>(Path);
    }

    protected override ExampleState Reduce(ExampleState state, Action topLevelAction)
    {
        return topLevelAction switch
        {
            ChangedHeaderText action => state with { HeaderText = action.HeaderText },
            DecrementCounter _ => state with { Counter = state.Counter - 1 },
            IncrementCounter _ => state with { Counter = state.Counter + 1 },
            UndoUndoableString _ => state with { UndoableString = state.UndoableString.Undo() },
            RedoUndoableString _ => state with { UndoableString = state.UndoableString.Redo() },
            SetUndoableString action => state with { UndoableString = state.UndoableString.Set(action.NewValue) },
            SubStateUpdater action => state with { Substate = ReduceSubStateUpdater(state.Substate, action) },
            _ => state,
        };
    }
}