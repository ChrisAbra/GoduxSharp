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
        var script = GD.Load<Script>("src/Plugins/Nodes/AppState.cs");
        AddCustomType("AppState", "AppState", script, null);
    }

    public override void _ExitTree()
    {
        // Clean-up of the plugin goes here.
        // Always remember to remove it from the engine when deactivated.
        RemoveCustomType("AppState");
    }
}
#endif