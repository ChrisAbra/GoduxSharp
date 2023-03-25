
using System.Collections.Concurrent;
using System.Reflection;
using System.Collections;
using Godot;

namespace Godux;

public abstract partial class StateStore<T> : Node
    where T : State
{
    public T CurrentState { get; protected set; }

    public delegate void Subscriber(PropertyInfo propertyInfo, object oldValue, object newValue);

    public static StateStore<T> Instance { get; protected set; }

    protected virtual string Path => "/root/AppState";

    public abstract override void _Ready();

    private PropertyInfo[] CachedProperties;

    protected abstract T Reduce(T state, Action action);

    private readonly ConcurrentDictionary<PropertyInfo, Subscriber> Subscribers = new();
    private readonly List<ChangedProperty> changedProperties = new();

    public void Dispatch(Action action)
    {
        var newState = Reduce(CurrentState,action);
        HandleStateUpdate(CurrentState, newState);
    }

    public void HandleStateUpdate(T oldState, T newState)
    {
        if (oldState == newState)
        {
            return;
        }
        CurrentState = newState;
        NotifySubscribers(GetChangedValues(oldState, newState));
    }

    private void NotifySubscribers(List<ChangedProperty> changedProperties)
    {
        foreach (var prop in changedProperties)
        {
            Subscribers.TryGetValue(prop.propertyInfo, out Subscriber subscribers);
            Task.Run(() => subscribers?.Invoke(prop.propertyInfo, prop.oldValue, prop.newValue));
        }
    }

    private record ChangedProperty
    {
        public PropertyInfo propertyInfo;
        public object oldValue;
        public object newValue;
    }
    private List<ChangedProperty> GetChangedValues(T oldState, T newState)
    {
        changedProperties.Clear();

        CachedProperties ??= oldState.GetType().GetProperties();
        foreach (var prop in CachedProperties)
        {
            var oldValue = prop.GetValue(oldState);
            var newValue = prop.GetValue(newState);

            dynamic newValueTyped = Convert.ChangeType(newValue, prop.PropertyType);
            dynamic oldValueTyped = Convert.ChangeType(oldValue, prop.PropertyType);

            if (newValueTyped != oldValueTyped)
            {
                GD.Print("+++ Changed:", prop.Name, " - ", newValue);
                changedProperties.Add(new ChangedProperty { propertyInfo = prop, oldValue = oldValue, newValue = newValue });
            }
        }
        return changedProperties;
    }

    public void AddSubscriber(PropertyInfo propertyInfo, Subscriber newSubscriber)
    {
        Subscribers.TryGetValue(propertyInfo, out Subscriber existingSubscribers);
        existingSubscribers += newSubscriber;
        Subscribers.TryAdd(propertyInfo, existingSubscribers);
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
            // if the delegate is not provided/overrden - provide a default one
            var _subscriber = subscriber ?? ((statePropertyInfo, oldValue, newValue) => 
            SetNodesWithProperty(node, wiredProp.Info, newValue, wiredProp.Attribute.NodePath, wiredProp.Attribute.NodeProperty));
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
