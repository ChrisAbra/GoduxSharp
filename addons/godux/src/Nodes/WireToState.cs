using System.Reflection;
using Godot;

namespace Godux;

public partial class WireToState : Node
{

    [Export]
    public string TargetProperty;
    [Export]
    public string StatePropertyPath;

    private Node Parent => GetParent();
    private PropertyInfo targetProperty;
    private PropertyInfo stateProperty;
    private StateStore store;


    public override void _Ready()
    {
        if(TargetProperty == null || StatePropertyPath == null){
            GD.PushWarning("Target or State property on node:", Parent, " not set");
            return;
        }
        store = AppState.Store;
        stateProperty = store.GetStatePropertyFromName(StatePropertyPath);
        targetProperty = Parent.GetType().GetProperty(TargetProperty);

        var value = store.GetStateValue(stateProperty);
        SetTargetProperty(value);
        store.AddSubscriber(StatePropertyPath, Subscriber);
    }

    private void Subscriber(PropertyInfo info, State state, object oldValue, object newValue)
    {
        SetTargetProperty(newValue);
    }

    private void SetTargetProperty(object newValue)
    {
        var targetType = targetProperty.PropertyType;

        GD.Print(targetType, stateProperty.PropertyType);

        if(targetType != stateProperty.PropertyType){
            if(targetType == typeof(string)){
                targetProperty?.SetValue(Parent, newValue.ToString());
            }
        }
        else {
            targetProperty?.SetValue(Parent, newValue);
        }
    }

    public override void _ExitTree()
    {
        //store.RemoveSubscriber(stateProperty,Subscriber);
    }
}