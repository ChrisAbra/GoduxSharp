using System.Collections.Immutable;

using Godux;

public partial class ExampleStateStore : StateStore<ExampleState>
{
    protected override string Path => "/root/AppState";

    public record ChangedHeaderText(string HeaderText) : Action;
    public record IncrementCounter() : Action;
    public record DecrementCounter() : Action;

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
            return state with { Counter = counter, CounterString = counter.ToString() };
        });
        On(typeof(IncrementCounter), (state, _) =>
        {
            var counter = state.Counter + 1;
            return state with { Counter = counter, CounterString = counter.ToString() };
        });
    }
}