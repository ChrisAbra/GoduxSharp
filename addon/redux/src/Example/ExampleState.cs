using System.Collections.Immutable;

namespace Redux.Example;


public partial record ExampleState : State
{
    public string HeaderText { get; set; } = "First Header Text";

    public record UpdateHeaderText : StoreAction
    {
        public string newText;

        public override State Reducer(State state)
        {
            if (state is ExampleState eState)
            {
                return eState with { HeaderText = newText };
            }
            else
            {
                return state;
            }
        }
    }
}
