using System.Collections.Immutable;
public record ExampleState : Godux.State
{
    public string HeaderText { get; init; } = "Default State Value";
    public ExampleSubState CounterSubState {get;init;} = new();
}