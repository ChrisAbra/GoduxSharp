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

public static class WirePropertyNodeExtension
{
    public static void ConnectAttributes(this Node node, Store store)
    {
        ConnectAttributes(node, store.CurrentState, (propertyInfo, oldValue, newValue) => {
            GD.Print(propertyInfo);
            GD.Print(oldValue);
            GD.Print(newValue);
            SetPropertyValue(node,propertyInfo,newValue);
        });
    }
    public static void ConnectAttributes(this Node node, Store store, HandleAttributesUpdate handleAttributesUpdate)
    {
        ConnectAttributes(node, store.CurrentState, handleAttributesUpdate);
    }

    private static void ConnectAttributes(this Node node, State state, HandleAttributesUpdate handleAttributesUpdate)
    {
        var wiredProperties = from property in node.GetType().GetProperties()
                              let attribute = property.GetCustomAttributes(typeof(WirePropertyAttribute), true)
                              where attribute.Length == 1
                              select new { Info = property, Attribute = attribute[0] as WirePropertyAttribute };

        foreach (var wiredProperty in wiredProperties)
        {
            SetPropertyFromState(node, wiredProperty.Info, state, wiredProperty.Attribute);
            state.Subscribe(wiredProperty.Attribute.StatePropertyName, wiredProperty.Info, handleAttributesUpdate);
        }
    }

    private static void SetPropertyFromState(Node node, PropertyInfo propertyInfo, State state, WirePropertyAttribute attribute){
        var statePropertyName = attribute.StatePropertyName;
        var value = state.GetValue(statePropertyName);
        node.SetPropertyValue(propertyInfo, value);
    }

    public static void SetPropertyValue(this Node node, PropertyInfo propertyInfo, object value){
        var attribute = propertyInfo.GetCustomAttribute<WirePropertyAttribute>();
        node.SetPropertyValue(propertyInfo,value,attribute);
    }
    private static void SetPropertyValue(this Node node, PropertyInfo propertyInfo, object value, WirePropertyAttribute attribute){
        propertyInfo.SetValue(node, value);
        if(attribute.NodePath != null && attribute.NodeProperty != null){
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