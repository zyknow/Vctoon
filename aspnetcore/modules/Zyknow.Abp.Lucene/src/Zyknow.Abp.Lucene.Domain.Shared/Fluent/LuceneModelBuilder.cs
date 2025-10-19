using System.Linq.Expressions;
using System.Globalization;

namespace Zyknow.Abp.Lucene.Fluent;

/// <summary>
/// Lucene 索引模型构建器（Fluent DSL）。
/// 用于以流式 API 注册实体及其参与索引的字段配置，并最终产出搜索描述符集合。
/// </summary>
public sealed class LuceneModelBuilder
{
    private readonly Dictionary<Type, Descriptors.EntitySearchDescriptor> _descriptors = new();

    /// <summary>
    /// 注册一个实体 <typeparamref name="T"/> 的索引与搜索配置。
    /// </summary>
    /// <param name="configure">针对实体的字段级配置委托。</param>
    /// <param name="indexName">索引名称；默认为实体类型名。</param>
    /// <returns>返回构建器以便继续链式调用。</returns>
    public LuceneModelBuilder Entity<T>(Action<EntitySearchBuilder<T>> configure, string? indexName = null)
    {
        var b = new EntitySearchBuilder<T>(indexName);
        configure(b);
        _descriptors[typeof(T)] = b.Build();
        return this;
    }

    /// <summary>
    /// 构建并返回所有已注册实体的搜索描述符集合。
    /// </summary>
    public IReadOnlyDictionary<Type, Descriptors.EntitySearchDescriptor> Build() => _descriptors;
}

/// <summary>
/// 实体搜索配置构建器。
/// 通过字段选择器与委托，为实体定义参与索引/搜索的字段及其行为（存储、权重、关键字等）。
/// </summary>
public sealed class EntitySearchBuilder<T>
{
    private readonly Descriptors.EntitySearchDescriptor _descriptor;

    /// <summary>
    /// 初始化当前实体的搜索描述符。
    /// </summary>
    /// <param name="indexName">索引名称；默认使用实体类型名。</param>
    public EntitySearchBuilder(string? indexName) => _descriptor = new Descriptors.EntitySearchDescriptor(typeof(T), indexName);

    /// <summary>
    /// 为实体属性添加一个参与索引/搜索的字段。
    /// </summary>
    /// <param name="selector">属性选择器，例如 <c>x =&gt; x.Title</c>。</param>
    /// <param name="configure">字段级配置委托（可设置存储、权重、关键字、自动补全等）。</param>
    /// <remarks>
    /// 字段默认名来自属性名解析（例如 <c>Title</c>）；可通过 <see cref="FieldBuilder.Name(string)"/> 重命名。
    /// </remarks>
    public EntitySearchBuilder<T> Field(Expression<Func<T, object>> selector, Action<FieldBuilder>? configure = null)
    {
        var fb = new FieldBuilder(selector);
        configure?.Invoke(fb);
        _descriptor.Fields.Add(fb.Build());
        return this;
    }

    /// <summary>
    /// 添加一个通过委托计算得到的值字段（非直接属性）。
    /// </summary>
    /// <param name="valueSelector">值选择器，例如 <c>x =&gt; $"{x.Author} - {x.Title}"</c>。</param>
    /// <param name="configure">字段级配置委托。</param>
    /// <remarks>
    /// 该字段默认名为 <c>"Value"</c>，可通过 <see cref="FieldBuilder.Name(string)"/> 显式命名。
    /// 适用于将多个属性拼接为一个搜索字段或生成派生文本。
    /// </remarks>
    public EntitySearchBuilder<T> ValueField(Func<T, string> valueSelector, Action<FieldBuilder>? configure = null)
    {
        var fb = new FieldBuilder(valueSelector);
        configure?.Invoke(fb);
        _descriptor.Fields.Add(fb.Build());
        return this;
    }

    /// <summary>
    /// 完成实体的字段配置并返回搜索描述符。
    /// </summary>
    public Descriptors.EntitySearchDescriptor Build() => _descriptor;
}

/// <summary>
/// 字段级配置构建器。
/// 为字段设置权重（Boost）、是否存储（Store）、命名（Name）、是否按关键字索引（Keyword）、自动补全（Autocomplete）和词条向量存储等。
/// </summary>
public sealed class FieldBuilder
{
    private readonly Descriptors.FieldDescriptor _field;

    /// <summary>
    /// 通过属性选择器初始化字段描述符，字段名将尝试解析为属性名。
    /// </summary>
    public FieldBuilder(LambdaExpression selector) => _field = new Descriptors.FieldDescriptor(selector);

    /// <summary>
    /// 通过值委托初始化字段描述符，默认字段名为 <c>"Value"</c>。
    /// </summary>
    public FieldBuilder(Delegate valueSelector) => _field = new Descriptors.FieldDescriptor(valueSelector);

    /// <summary>
    /// 是否存储字段原始值（用于在搜索结果中返回展示）。
    /// </summary>
    /// <param name="enabled">默认 <c>true</c>。</param>
    /// <remarks>
    /// 开启后，该字段的原始文本会写入索引并在命中结果的 <c>Payload</c> 中返回。
    /// 关闭时仍可检索，但结果中不返回该字段值。
    /// </remarks>
    public FieldBuilder Store(bool enabled = true)
    { _field.Store = enabled; return this; }

    /// <summary>
    /// 显式设置字段名。
    /// </summary>
    /// <param name="name">字段名。</param>
    /// <remarks>
    /// 可用于重命名默认的属性名或委托字段的 <c>"Value"</c> 名称，以便查询/展示更一致。
    /// </remarks>
    public FieldBuilder Name(string name)
    { _field.Name = name; return this; }

    /// <summary>
    /// 将字段按关键字（不分词）进行索引，适合精确匹配的标识符或标签。
    /// </summary>
    public FieldBuilder Keyword()
    { _field.Keyword = true; return this; }

    /// <summary>
    /// 关键字字段统一小写存储（便于大小写不敏感查询，配合规范化）。
    /// </summary>
    public FieldBuilder LowerCaseKeyword()
    { _field.Keyword = true; _field.LowerCaseKeyword = true; _field.LowerCaseCulture = CultureInfo.InvariantCulture; return this; }

    /// <summary>
    /// 关键字字段统一小写存储（指定文化）。
    /// </summary>
    public FieldBuilder LowerCaseKeyword(CultureInfo culture)
    { _field.Keyword = true; _field.LowerCaseKeyword = true; _field.LowerCaseCulture = culture; return this; }

    // 新增：声明数值/日期类型（用于 NumericRangeQuery 等）。
    public FieldBuilder AsInt32()
    { _field.NumericKind = Descriptors.LuceneNumericKind.Int32; return this; }

    public FieldBuilder AsInt64()
    { _field.NumericKind = Descriptors.LuceneNumericKind.Int64; return this; }

    public FieldBuilder AsDateEpochMillis()
    { _field.NumericKind = Descriptors.LuceneNumericKind.DateEpochMillis; return this; }

    public FieldBuilder AsDateEpochSeconds()
    { _field.NumericKind = Descriptors.LuceneNumericKind.DateEpochSeconds; return this; }

    // 自动补全
    public FieldBuilder Autocomplete(int minGram = 1, int maxGram = 20)
    { _field.Autocomplete = (minGram, maxGram); return this; }

    /// <summary>
    /// 存储词条向量（用于高亮等场景）。
    /// </summary>
    public FieldBuilder StoreTermVectors(bool positions = true, bool offsets = true)
    { _field.StoreTermVectors = (positions, offsets); return this; }

    /// <summary>
    /// 声明该字段依赖的属性名（用于自动投影时选择列）。
    /// </summary>
    public FieldBuilder Depends(params string[] propertyNames)
    { _field.Depends.AddRange(propertyNames); return this; }

    /// <summary>
    /// 将该字段标记为仅存储值但不参与查询（不被索引）。
    /// </summary>
    public FieldBuilder StoreOnly()
    { _field.Searchable = false; _field.Store = true; return this; }

    internal Descriptors.FieldDescriptor Build() => _field;
}