using Godot;

namespace Godux;

[Tool]
public partial class AppState : Node
{
    [Export]
    public StateStore<State> Store;
    private State State => Store?.CurrentState;
    public override void _Ready()
    {
        GD.Print("Custom node AppState Ready");
    }
}