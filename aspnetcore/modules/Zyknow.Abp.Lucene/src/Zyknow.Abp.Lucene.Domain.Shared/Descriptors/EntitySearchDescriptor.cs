namespace Zyknow.Abp.Lucene.Descriptors;

public class EntitySearchDescriptor(Type type, string? indexName)
{
    public Type EntityType { get; } = type;
    public string IndexName { get; } = indexName ?? type.Name;
    public List<FieldDescriptor> Fields { get; } = [];
    public string IdFieldName { get; set; } = "Id";
}