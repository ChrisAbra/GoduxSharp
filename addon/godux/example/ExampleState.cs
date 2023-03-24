using System.Collections.Immutable;
using Godux;
public record ExampleState : Godux.State
{
    public string HeaderText { get; init; } = "Default State Value";
    public int Counter { get; init; } = 12;
    public string CounterString => Counter.ToString();

    public Undoable<string> UndoableString {get;init;} = "Undoable Header";
    public string UndoableStringPresent => UndoableString.Present;

}