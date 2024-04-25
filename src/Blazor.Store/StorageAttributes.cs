namespace Blazor.Store;

[AttributeUsage(AttributeTargets.Class)]
public class SessionStorageAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class LocalStorageAttribute : Attribute
{
}