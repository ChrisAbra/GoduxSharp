using System.Reflection;

namespace Redux;
public delegate void HandleAttributesUpdate(PropertyInfo property, object oldValue, object newValue);
public record WiredProperty {
    public PropertyInfo WiredPropertyInfo;
    public HandleAttributesUpdate Listener;

    public WiredProperty (PropertyInfo propertyInfo,HandleAttributesUpdate listener ){
        WiredPropertyInfo = propertyInfo;
        Listener = listener;
    }
}
public abstract record State
{
    public Dictionary<string,List<WiredProperty>> AttributeListeners = new ();

    public void Subscribe(string statePropertyName, PropertyInfo wiredPropertyInfo, HandleAttributesUpdate listener ){
        var wiredProperties = AttributeListeners.GetValueOrDefault(statePropertyName) ?? new List<WiredProperty>();
        wiredProperties.Add(new WiredProperty(wiredPropertyInfo,listener));
        AttributeListeners.Add(statePropertyName,wiredProperties);
    }
    public State NewState(string propertyName, object newValue){
        var newState = this with { };
        var property = newState.GetType().GetProperty(propertyName);
        var oldValue = property.GetValue(newState);

        if(newValue == oldValue){ // if the value is unchanged dont fire the listeners
            return newState;
        }

        property.SetValue(newState,newValue);

        foreach(var wiredProperty in AttributeListeners.GetValueOrDefault(propertyName)){
            GD.Print(wiredProperty);
            wiredProperty.Listener.Invoke(wiredProperty.WiredPropertyInfo,oldValue,newValue);
        }
        return  newState;
    }
    public object GetValue(string propertyName)
    {
        return this.GetType().GetProperty(propertyName).GetValue(this);
    }
}
