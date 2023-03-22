
namespace Godux;

[AttributeUsage(AttributeTargets.Property)]
public class WireToStateAttribute : Attribute
{
    public string StatePropertyName { get; }
    public string NodePath { get; }
    public string NodeProperty { get; }
    public WireToStateAttribute(string statePropertyName)
    {
        this.StatePropertyName = statePropertyName;
    }
    public WireToStateAttribute(string statePropertyName, string nodePath, string nodeProperty)
    {
        this.StatePropertyName = statePropertyName;
        this.NodePath = nodePath;
        this.NodeProperty = nodeProperty;
    }
    public WireToStateAttribute(string[] statePropertyPaths, string nodePath, string nodeProperty)
    {
        this.StatePropertyName = string.Join(".",statePropertyPaths);
        this.NodePath = nodePath;
        this.NodeProperty = nodeProperty;
    }

}