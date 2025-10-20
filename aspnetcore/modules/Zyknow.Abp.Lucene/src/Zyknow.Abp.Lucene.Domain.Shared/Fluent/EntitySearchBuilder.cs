using System.Linq.Expressions;
using Zyknow.Abp.Lucene.Descriptors;

namespace Zyknow.Abp.Lucene.Fluent;

/// <summary>
/// 实体搜索配置构建器。
/// 通过字段选择器与委托，为实体定义参与索引/搜索的字段及其行为（存储、权重、关键字等）。
/// </summary>
public class EntitySearchBuilder<T>
{
    private readonly EntitySearchDescriptor _descriptor;

    /// <summary>
    /// 初始化当前实体的搜索描述符。
    /// </summary>
    /// <param name="indexName">索引名称；默认使用实体类型名。</param>
    public EntitySearchBuilder(string? indexName)
    {
        _descriptor = new EntitySearchDescriptor(typeof(T), indexName);
    }

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
    public EntitySearchDescriptor Build()
    {
        return _descriptor;
    }
}