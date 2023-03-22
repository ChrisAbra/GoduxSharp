using System.Collections.Immutable;
public record ExampleState : Godux.State
{
    public string HeaderText { get; init; } = "Default State Value";
    public int Counter { get; init; } = 12;
    public string CounterString => Counter.ToString();
}