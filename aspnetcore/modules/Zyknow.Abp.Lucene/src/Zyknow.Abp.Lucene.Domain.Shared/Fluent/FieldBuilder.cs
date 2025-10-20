using System.Globalization;
using System.Linq.Expressions;
using Zyknow.Abp.Lucene.Descriptors;

namespace Zyknow.Abp.Lucene.Fluent;

/// <summary>
/// 字段级配置构建器。
/// 为字段设置权重（Boost）、是否存储（Store）、命名（Name）、是否按关键字索引（Keyword）、自动补全（Autocomplete）和词条向量存储等。
/// </summary>
public class FieldBuilder
{
    private readonly FieldDescriptor _field;

    /// <summary>
    /// 通过属性选择器初始化字段描述符，字段名将尝试解析为属性名。
    /// </summary>
    public FieldBuilder(LambdaExpression selector)
    {
        _field = new FieldDescriptor(selector);
    }

    /// <summary>
    /// 通过值委托初始化字段描述符，默认字段名为 <c>"Value"</c>。
    /// </summary>
    public FieldBuilder(Delegate valueSelector)
    {
        _field = new FieldDescriptor(valueSelector);
    }

    public FieldBuilder Store(bool enabled = true)
    {
        _field.Store = enabled;
        return this;
    }

    public FieldBuilder Name(string name)
    {
        _field.Name = name;
        return this;
    }

    public FieldBuilder Keyword()
    {
        _field.Keyword = true;
        return this;
    }

    public FieldBuilder LowerCaseKeyword()
    {
        _field.Keyword = true;
        _field.LowerCaseKeyword = true;
        _field.LowerCaseCulture = CultureInfo.InvariantCulture;
        return this;
    }

    public FieldBuilder LowerCaseKeyword(CultureInfo culture)
    {
        _field.Keyword = true;
        _field.LowerCaseKeyword = true;
        _field.LowerCaseCulture = culture;
        return this;
    }

    public FieldBuilder AsInt32()
    {
        _field.NumericKind = LuceneNumericKind.Int32;
        return this;
    }

    public FieldBuilder AsInt64()
    {
        _field.NumericKind = LuceneNumericKind.Int64;
        return this;
    }

    public FieldBuilder AsDateEpochMillis()
    {
        _field.NumericKind = LuceneNumericKind.DateEpochMillis;
        return this;
    }

    public FieldBuilder AsDateEpochSeconds()
    {
        _field.NumericKind = LuceneNumericKind.DateEpochSeconds;
        return this;
    }

    public FieldBuilder Autocomplete(int minGram = 1, int maxGram = 20)
    {
        _field.Autocomplete = (minGram, maxGram);
        return this;
    }

    public FieldBuilder StoreTermVectors(bool positions = true, bool offsets = true)
    {
        _field.StoreTermVectors = (positions, offsets);
        return this;
    }

    public FieldBuilder Depends(params string[] propertyNames)
    {
        _field.Depends.AddRange(propertyNames);
        return this;
    }

    public FieldBuilder StoreOnly()
    {
        _field.Searchable = false;
        _field.Store = true;
        return this;
    }

    internal FieldDescriptor Build()
    {
        return _field;
    }
}