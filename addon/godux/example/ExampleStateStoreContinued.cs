namespace Godux.Example;


public partial class ExampleStateStore : Godux.StateStore<ExampleState>
{
    public record SubStateUpdater : Action;
    public record SubStateStringUpdater(string SubstateString) : SubStateUpdater;


    private static ExampleSubState ReduceSubStateUpdater(ExampleSubState substate, SubStateUpdater subaction)
    {
        return subaction switch
        {
            SubStateStringUpdater action => substate with { SubstateValue = action.SubstateString },
            _ => substate,
        };
    }
}