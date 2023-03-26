#if TOOLS
using Godot;

namespace Godux;

[Tool]
public partial class WireNodesPlugin : EditorPlugin
{
    public override void _EnterTree()
    {
        // Initialization of the plugin goes here.
        // Add the new type with a name, a parent type, a script and an icon.
        var connection = GD.Load<Script>("addons/godux/src/Nodes/StateStoreConnection/StateStoreConnection.cs");

        AddCustomType("StateStoreConnection", "Node", connection, null);

        var wire = GD.Load<Script>("addons/godux/src/Nodes/WireToState.cs");
        AddCustomType("WireToState", "Node", wire, null);

    }

    public override void _ExitTree()
    {
        // Clean-up of the plugin goes here.
        // Always remember to remove it from the engine when deactivated.
        RemoveCustomType("StateStoreConnection");
        RemoveCustomType("WireToState");
    }
}
#endif