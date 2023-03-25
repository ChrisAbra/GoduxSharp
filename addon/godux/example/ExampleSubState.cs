using System.Collections.Immutable;

namespace Godux.Example;

public record ExampleSubState : Godux.State
{
    public string SubstateValue { get; set; } = "Default substate value";
}