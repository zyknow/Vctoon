namespace Blazor.Store;

[AttributeUsage(AttributeTargets.Class)]
public class IgnorePropertyStoreAttribute(params string[] names) : Attribute
{
    public string[] Names { get; } = names;
}