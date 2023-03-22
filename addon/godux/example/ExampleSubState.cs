using System.Collections.Immutable;
public record ExampleSubState : Godux.State
{
    public int Counter { get; init; } = 12;
    public string CounterString => Counter.ToString();
}