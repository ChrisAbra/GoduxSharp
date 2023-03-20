using System.Collections.Immutable;

namespace Redux.Example;


public partial record ExampleState : State
{
    public string HeaderText { get; private set; } = "First Header Text";
    public record UpdateHeaderText : StoreAction
    {
        public string newText;

        public ExampleState Reducer(State state)
        {
            return (ExampleState)state.NewState(nameof(ExampleState.HeaderText), newText);
        }
    }
}
