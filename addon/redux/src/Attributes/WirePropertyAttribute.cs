using System.Reflection;
namespace Redux;

[AttributeUsage(AttributeTargets.Property)]
public class WirePropertyAttribute : Attribute
{
    public string StatePropertyName { get; }
    public string NodePath { get; }
    public string NodeProperty { get; }
    public WirePropertyAttribute(string statePropertyName)
    {
        this.StatePropertyName = statePropertyName;
    }
    public WirePropertyAttribute(string statePropertyName, string nodePath, string nodeProperty)
    {
        this.StatePropertyName = statePropertyName;
        this.NodePath = nodePath;
        this.NodeProperty = nodeProperty;
    }
}

public delegate void HandleAttributesUpdate(PropertyInfo property, object oldValue, object newValue);

public static class WirePropertyNodeExtension
{
    public static void ConnectAttributes(this Node node, Store store)
    {
        ConnectAttributes(node, store.CurrentState, (property, oldValue, newValue) => property.SetValue(node, newValue));
    }
    public static void ConnectAttributes(this Node node, Store store, HandleAttributesUpdate handleAttributesUpdate)
    {
        ConnectAttributes(node, store.CurrentState, (property, oldValue, newValue) => handleAttributesUpdate.Invoke(property,oldValue,newValue));
    }


    public record WirePropertyAttributeContainer
    {
        public PropertyInfo Info;
        public WirePropertyAttribute Attribute;
    }
    private static void ConnectAttributes(this Node node, State state, HandleAttributesUpdate handleAttributesUpdate)
    {
        var wiredProperties = from property in node.GetType().GetProperties()
                              let attribute = property.GetCustomAttributes(typeof(WirePropertyAttribute), true)
                              where attribute.Length == 1
                              select new WirePropertyAttributeContainer { Info = property, Attribute = attribute[0] as WirePropertyAttribute };

        foreach (var wiredProperty in wiredProperties)
        {
            SetPropertyFromState(node, wiredProperty, state);
        }
    }

    private static void SetPropertyFromState(Node node, WirePropertyAttributeContainer wiredProperty, State state)
    {
        var attribute = wiredProperty.Attribute;
        var statePropertyName = attribute.StatePropertyName;
        var value = state.GetValue(statePropertyName);

        wiredProperty.Info.SetValue(node, value);
        if (attribute.NodePath != null && attribute.NodeProperty != null)
        {
            node.SetNodesWithProperty(attribute.NodePath, attribute.NodeProperty, value);
        }
    }

    private static void SetNodesWithProperty(this Node node, string nodePath, string nodePropertyName, object value)
    {
        var childNode = node.GetNode(nodePath);
        foreach (var prop in childNode.GetType().GetProperties())
        {
            if (prop.Name == nodePropertyName)
            {
                prop.SetValue(childNode, value);
                break;
            }
        }
    }
}