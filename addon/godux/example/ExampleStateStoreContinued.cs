namespace Godux.Example;

public partial class ExampleStateStore : Godux.StateStore<ExampleState>
{

    public record SubStateUpdater(string SubstateValue) : Action;

    public void ContinuedReducers()
    {
        On(typeof(SubStateUpdater), (state, action) =>
        {
            var subStateUpdater = action as SubStateUpdater;
            return state with { Substate = state.Substate with { SubstateValue = subStateUpdater.SubstateValue } };
        });

    }
}