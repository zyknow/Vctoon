using Zyknow.Abp.Lucene.Descriptors;

namespace Zyknow.Abp.Lucene.Fluent;

/// <summary>
/// Lucene 索引模型构建器（Fluent DSL）。
/// 用于以流式 API 注册实体及其参与索引的字段配置，并最终产出搜索描述符集合。
/// </summary>
public class LuceneModelBuilder
{
    private readonly Dictionary<Type, EntitySearchDescriptor> _descriptors = new();

    public LuceneModelBuilder Entity<T>(Action<EntitySearchBuilder<T>> configure, string? indexName = null)
    {
        var b = new EntitySearchBuilder<T>(indexName);
        configure(b);
        _descriptors[typeof(T)] = b.Build();
        return this;
    }

    public IReadOnlyDictionary<Type, EntitySearchDescriptor> Build()
    {
        return _descriptors;
    }
}