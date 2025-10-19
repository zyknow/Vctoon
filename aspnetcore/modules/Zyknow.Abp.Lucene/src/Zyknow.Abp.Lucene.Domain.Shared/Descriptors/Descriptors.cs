using System.Linq.Expressions;
using System.Globalization;

namespace Zyknow.Abp.Lucene.Descriptors;

public sealed class EntitySearchDescriptor
{
    public Type EntityType { get; }
    public string IndexName { get; }
    public List<FieldDescriptor> Fields { get; } = new();
    public string IdFieldName { get; set; } = "Id";

    public EntitySearchDescriptor(Type type, string? indexName)
    {
        EntityType = type;
        IndexName = indexName ?? type.Name;
    }
}

public enum LuceneNumericKind
{
    None,
    Int32,
    Int64,
    DateEpochMillis,
    DateEpochSeconds
}

public sealed class FieldDescriptor
{
    public string Name { get; set; } = string.Empty;
    public LambdaExpression? Selector { get; }
    public Delegate? ValueSelector { get; }
    public float Boost { get; set; } = 1.0f;
    public bool Store { get; set; } = false;
    public bool Keyword { get; set; } = false;
    public bool LowerCaseKeyword { get; set; } = false;
    public CultureInfo? LowerCaseCulture { get; set; }
    public (int min, int max)? Autocomplete { get; set; }
    public (bool positions, bool offsets)? StoreTermVectors { get; set; }
    public List<string> Depends { get; } = new();
    // 是否参与查询（被索引）。默认 true。
    public bool Searchable { get; set; } = true;
    // 新增：数值/日期类型配置（用于 NumericRangeQuery 与数值等值）。
    public LuceneNumericKind NumericKind { get; set; } = LuceneNumericKind.None;

    public FieldDescriptor(LambdaExpression selector)
    {
        Selector = selector;
        Name = InferName(selector);
    }

    public FieldDescriptor(Delegate valueSelector)
    {
        ValueSelector = valueSelector;
        Name = "Value";
    }

    internal static string InferName(LambdaExpression exp)
    {
        // x => x.Prop
        if (exp.Body is MemberExpression m) return m.Member.Name;
        if (exp.Body is UnaryExpression u && u.Operand is MemberExpression um) return um.Member.Name;
        return "Field";
    }
}