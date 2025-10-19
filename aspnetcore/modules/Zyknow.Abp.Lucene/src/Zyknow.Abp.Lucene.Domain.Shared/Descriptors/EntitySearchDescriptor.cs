namespace Zyknow.Abp.Lucene.Descriptors;

public sealed class EntitySearchDescriptor
{
    public EntitySearchDescriptor(Type type, string? indexName)
    {
        EntityType = type;
        IndexName = indexName ?? type.Name;
    }

    public Type EntityType { get; }
    public string IndexName { get; }
    public List<FieldDescriptor> Fields { get; } = new();
    public string IdFieldName { get; set; } = "Id";
}