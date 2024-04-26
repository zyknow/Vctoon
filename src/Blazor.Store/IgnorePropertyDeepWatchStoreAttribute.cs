namespace Blazor.Store;

[AttributeUsage(AttributeTargets.Class)]
public class IgnorePropertyDeepWatchStoreAttribute(params string[] names) : Attribute
{
    public string[] Names { get; } = names;
}