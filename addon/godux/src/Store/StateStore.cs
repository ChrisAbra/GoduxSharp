
using System.Collections.Concurrent;
using System.Reflection;
using System.Collections;
using Godot;

namespace Godux;

public abstract partial class StateStore<T> : Node
    where T : State
{
    public T CurrentState { get; protected set; }

    public delegate void Subscriber(PropertyInfo propertyInfo, State state, object oldValue, object newValue);

    public static StateStore<T> Instance { get; protected set; }

    protected virtual string Path => "/root/AppState";
    public abstract void InitalizeState();

    public override void _Ready(){
        Instance = this.GetNode<StateStore<T>>(Path);
        InitalizeState();
    }

    protected abstract T Reduce(T state, Action action);

    private readonly ConcurrentDictionary<PropertyInfo, Subscriber> Subscribers = new();

    public void Dispatch(Action action)
    {
        var newState = Reduce(CurrentState, action);
        HandleStateUpdate(CurrentState, newState);
    }

    public void HandleStateUpdate(T oldState, T newState)
    {
        if (oldState == newState)
        {
            return;
        }
        CurrentState = newState;
        var changedProperties = CurrentState.GetChangedValues(oldState);

        NotifySubscribers(changedProperties);
    }

    private void NotifySubscribers(List<State.ChangedProperty> changedProperties)
    {
        foreach (var prop in changedProperties)
        {
            Subscribers.TryGetValue(prop.propertyInfo, out Subscriber subscribers);
            subscribers?.Invoke(prop.propertyInfo, prop.state, prop.oldValue, prop.newValue);
        }
    }
    public void AddSubscriber(string propertyFullPath, Subscriber newSubscriber)
    {
        var propertyInfo = GetStatePropertyFromName(propertyFullPath);
        AddSubscriber(propertyInfo, newSubscriber);
    }

    public void AddSubscriber(PropertyInfo propertyInfo, Subscriber newSubscriber)
    {
        // if there is already a subscription list for this property, then add the new subscriber to it
        if (Subscribers.TryGetValue(propertyInfo, out Subscriber existingSubscribers))
        {
            Subscribers.TryUpdate(propertyInfo, (existingSubscribers + newSubscriber), existingSubscribers);
            return;
        }
        // otherwise extablish the new subscription list for this property
        Subscribers.TryAdd(propertyInfo, newSubscriber);
    }

    private PropertyInfo GetStatePropertyFromName(string propertyName)
    {
        PropertyInfo stateProperty = null;
        foreach (string path in propertyName.Split("."))
        {
            if (stateProperty is null)
            {
                stateProperty = CurrentState.GetType().GetProperty(path);
            }
            else
            {
                stateProperty = stateProperty.PropertyType.GetProperty(path);
            }
        }

        if(stateProperty == null){
            throw new Exception($"Property not found for '{propertyName}'. Check the property name is correct and that the property on the state is not a field instead");
        }

        return stateProperty;
    }

    private object GetStateValue(PropertyInfo propertyInfo)
    {
        return propertyInfo?.GetValue(CurrentState);
    }

    public void ConnectWiredAttributes(Node node, Subscriber subscriber = null)
    {
        var wiredProperties = from property in node.GetType().GetProperties()
                              let attributes = property.GetCustomAttributes(typeof(WireToStateAttribute), true)
                              where attributes.Length == 1
                              select new { Info = property, Attribute = attributes[0] as WireToStateAttribute };

        foreach (var wiredProp in wiredProperties)
        {
            var stateProperty = GetStatePropertyFromName(wiredProp.Attribute.StatePropertyName);
            var value = GetStateValue(stateProperty);

            if (wiredProp.Info.PropertyType != stateProperty.PropertyType)
            {
                throw new Exception("Type of wired property does not match state property. Change the type or extend the state and update the reducer functions");
            }

            SetNodesWithProperty(node, wiredProp.Info, value, wiredProp.Attribute.NodePath, wiredProp.Attribute.NodeProperty);

            void DefaultWiredSubscriber(PropertyInfo statePropertyInfo, State state, object _, object newValue){
                if(wiredProp.Attribute?.SubstateName != null){
                    Type substateType = Type.GetType(wiredProp.Attribute.SubstateName);
                    if(!substateType.IsAssignableFrom(state.GetType())) return; // Where the state doesnt match the expected type ignore.
                }
                SetNodesWithProperty(node, wiredProp.Info, newValue, wiredProp.Attribute.NodePath, wiredProp.Attribute.NodeProperty);
            }

            // if the delegate is not provided/overrden - provide a default one
            var _subscriber = subscriber ?? DefaultWiredSubscriber;
            AddSubscriber(stateProperty, _subscriber);
        }
    }

    static void SetNodesWithProperty(Node node, PropertyInfo nodePropertyInfo, object newValue, string childNodePath = null, string childNodePropertyName = null)
    {
        nodePropertyInfo.SetValue(node, newValue);

        if (childNodePath == null || childNodePropertyName == null)
        {
            return;
        }
        var childNode = node?.GetNode(childNodePath);
        if (childNode == null)
        {
            throw new Exception("Child node not found for WireToStateAttribute");
        }

        var childNodeProperty = from property in childNode.GetType().GetProperties()
                                where property.Name == childNodePropertyName
                                select property;

        var childProperty = childNodeProperty.First();
        if (childProperty.PropertyType != nodePropertyInfo.PropertyType)
        {
            throw new Exception("Type of wired property's child target does not match state property. Change the type or extend the state and update the reducer functions");
        }

        childProperty.SetValue(childNode, newValue);
    }
}
