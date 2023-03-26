
using System.Collections.Concurrent;
using System.Reflection;
using System.Collections;
using Godot;

namespace Godux;

public static class AppState {
    public static StateStore Store {get;set;}
}
public abstract partial class StateStore: Node {
    public abstract PropertyInfo GetStatePropertyFromName(string propertyName);
    public abstract object GetStateValue(PropertyInfo propertyInfo);
    public delegate void SubscriberObject(PropertyInfo propertyInfo, State state, object oldValue, object newValue);

    public abstract void Dispatch(Action action);
    public abstract void ConnectWiredAttributes(Node node, SubscriberObject subscriber = null);
    protected virtual string Path => "/root/AppState";
    public abstract void AddSubscriber(string propertyFullPath, SubscriberObject newSubscriber);
    public abstract void AddSubscriber(PropertyInfo propertyInfo, SubscriberObject newSubscriber);

}

public abstract partial class StateStore<T> : StateStore
    where T : State
{
    public T CurrentState { get; protected set; }

    public static StateStore<T> Instance { get; protected set; }

    public abstract void InitalizeState();

    public override void _Ready()
    {
        InitalizeState();
        Instance = GetInstance(); // Typed 
        AppState.Store = Instance; // Base
    }

    public StateStore<T> GetInstance(){
        return this.GetNode<StateStore<T>>(Path);
    }

    protected abstract T Reduce(T state, Action action);

    private readonly ConcurrentDictionary<PropertyInfo, SubscriberObject> Subscribers = new();

    public override void Dispatch(Action action)
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
            Subscribers.TryGetValue(prop.propertyInfo, out SubscriberObject subscribers);
            subscribers?.Invoke(prop.propertyInfo, prop.state, prop.oldValue, prop.newValue);
        }
    }
    public override void AddSubscriber(string propertyFullPath, SubscriberObject newSubscriber)
    {
        var propertyInfo = GetStatePropertyFromName(propertyFullPath);
        AddSubscriber(propertyInfo, newSubscriber);
    }

    public override void AddSubscriber(PropertyInfo propertyInfo, SubscriberObject newSubscriber)
    {
        Subscribers.TryGetValue(propertyInfo, out SubscriberObject existingSubscribers);
        existingSubscribers += newSubscriber;
        Subscribers.TryAdd(propertyInfo, existingSubscribers);
    }

    public override PropertyInfo GetStatePropertyFromName(string propertyName)
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

        if (stateProperty == null)
        {
            throw new Exception($"Property not found for '{propertyName}'. Check the property name is correct and that the property on the state is not a field instead");
        }

        return stateProperty;
    }

    public override object GetStateValue(PropertyInfo propertyInfo)
    {
        return propertyInfo?.GetValue(CurrentState);
    }

    public override void ConnectWiredAttributes(Node node, SubscriberObject subscriber = null)
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

            void DefaultWiredSubscriber(PropertyInfo statePropertyInfo, State state, object _, object newValue)
            {
                if (wiredProp.Attribute?.SubstateName != null)
                {
                    Type substateType = Type.GetType(wiredProp.Attribute.SubstateName);
                    if (!substateType.IsAssignableFrom(state.GetType())) return; // Where the state doesnt match the expected type ignore.
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
