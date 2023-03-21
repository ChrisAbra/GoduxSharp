
using System.Collections.Immutable;

public record ExampleState : State
{
    public string HeaderText {get; init;}

    public ImmutableArray<string> Pages {get;init;}
}