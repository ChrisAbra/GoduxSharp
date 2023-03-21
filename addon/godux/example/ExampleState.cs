using System.Collections.Immutable;

using Godux;
public record ExampleState : State
{
    public string HeaderText { get; init; } = "Default State Value";
    public int Counter { get; init; } = 0;
    public string CounterString { get; init; } = "0";
}