
using System.Collections.Concurrent;
using System.Reflection;
using Godot;

namespace Godux;

public abstract record State { }
public abstract partial class StateStore<T> : Node
    where T : State
{

    public abstract record Action;

    public T CurrentState { get; protected set; }

    protected readonly List<T> historicStates = new();

    private readonly Dictionary<Type, Reducer> Reducers = new();

    public delegate T Reducer(T state, Action action);

    public delegate void Subscriber(PropertyInfo propertyInfo, object oldValue, object newValue);

    protected bool KeepHistoricStates = false;

    public static StateStore<T> Instance { get; protected set; }

    protected virtual string Path => "/root/AppState";

    public abstract override void _Ready();

    private PropertyInfo[] CachedProperties;

    private ConcurrentDictionary<PropertyInfo, Subscriber> Subscribers = new();

    public void Dispatch(Action action)
    {
        Reducer reducer = Reducers[action.GetType()];
        var newState = reducer?.Invoke(CurrentState, action) ?? CurrentState;
        HandleStateUpdate(CurrentState, newState);
    }

    public void HandleStateUpdate(T oldState, T newState)
    {
        if (oldState == newState)
        {
            return;
        }
        if (KeepHistoricStates)
        {
            historicStates.Add(oldState);
        }
        NotifySubscribers(GetChangedValues(oldState, newState));
        CurrentState = newState;
    }

    private void NotifySubscribers(List<ChangedProperty> changedProperties)
    {
        foreach (var prop in changedProperties)
        {
            Subscribers.TryGetValue(prop.propertyInfo, out Subscriber subscribers);
            subscribers?.Invoke(prop.propertyInfo, prop.oldValue, prop.newValue);
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
        List<ChangedProperty> changedProperties = new();
        CachedProperties ??= oldState.GetType().GetProperties();
        foreach (var prop in CachedProperties)
        {
            object oldValue = prop.GetValue(oldState);
            object newValue = prop.GetValue(newState);
            if (oldValue != newValue)
            {
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

    protected void On(Type actionType, Reducer reducer)
    {
        Reducers.Add(actionType, reducer);
    }

    private PropertyInfo GetStatePropertyFromName(string propertyName)
    {
        return CurrentState.GetType().GetProperty(propertyName);
    }

    private object GetStateValue(string propertyName)
    {
        return GetStatePropertyFromName(propertyName)?.GetValue(CurrentState);
    }
    private object GetStateValue(PropertyInfo propertyInfo)
    {
        return propertyInfo?.GetValue(CurrentState);
    }


    public void ConnectWiredAttributes(Node node, Subscriber subscriber = null)
    {
        var wiredProperties = from property in node.GetType().GetProperties()
                              let attribute = property.GetCustomAttributes(typeof(WireToStateAttribute), true)
                              where attribute.Length == 1
                              select new { Info = property, Attribute = attribute[0] as WireToStateAttribute };
        foreach (var prop in wiredProperties)
        {
            var stateProperty = GetStatePropertyFromName(prop.Attribute.StatePropertyName);
            var value = GetStateValue(stateProperty);

            if (prop.Info.PropertyType != stateProperty.PropertyType)
            {
                throw new Exception("Type of wired property does not match state property. Change the type or extend the state and update the reducer functions");
            }

            SetNodesWithProperty(node, prop.Info, value, prop.Attribute.NodePath, prop.Attribute.NodeProperty);
            // if the delegate is not provided/overrden - provide a default one
            var _subscriber = subscriber ?? ((_, __, newValue) => SetNodesWithProperty(node, prop.Info, newValue, prop.Attribute.NodePath, prop.Attribute.NodeProperty));
            AddSubscriber(stateProperty, _subscriber);
        }
    }

    static void SetNodesWithProperty(Node node, PropertyInfo nodePropertyInfo, object newValue)
    {
        nodePropertyInfo.SetValue(node, newValue);
    }

    static void SetNodesWithProperty(Node node, PropertyInfo nodePropertyInfo, object newValue, string childNodePath = null, string childNodeProperty = null)
    {
        SetNodesWithProperty(node, nodePropertyInfo, newValue);
        if (childNodePath != null && childNodeProperty != null)
        {
            var childNode = node.GetNode(childNodePath);
            if (childNode == null)
            {
                throw new Exception("Child node not found for WireToStateAttribute");
            }
            foreach (var prop in childNode.GetType()?.GetProperties())
            {
                if (prop.Name == childNodeProperty)
                {
                    if (prop.PropertyType != nodePropertyInfo.PropertyType)
                    {
                        throw new Exception("Type of wired property's child target does not match state property. Change the type or extend the state and update the reducer functions");
                    }

                    prop.SetValue(childNode, newValue);
                    break;
                }
            }
        }
    }
}
