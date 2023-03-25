using System.Reflection;

namespace Godux;

public abstract record State
{
    private readonly List<ChangedProperty> changedProperties = new();

    public record ChangedProperty
    {
        public State state;
        public PropertyInfo propertyInfo;
        public object oldValue;
        public object newValue;
    }

    private PropertyInfo[] CachedProperties;

    public List<ChangedProperty> GetChangedValues(State oldState)
    {
        changedProperties.Clear();

        CachedProperties ??= this.GetType().GetProperties();
        foreach (var prop in CachedProperties)
        {
            var oldValue = prop.GetValue(oldState);
            var newValue = prop.GetValue(this);

            if(newValue is State substate){
                changedProperties.AddRange(substate.GetChangedValues(oldValue as State));
            }
            else{
                dynamic newValueTyped = Convert.ChangeType(newValue, prop.PropertyType);
                dynamic oldValueTyped = Convert.ChangeType(oldValue, prop.PropertyType);

                if (newValueTyped != oldValueTyped)
                {
                    changedProperties.Add(new ChangedProperty { state= this, propertyInfo = prop, oldValue = oldValue, newValue = newValue });
                }
            }
        }
        return changedProperties;
    }

}
