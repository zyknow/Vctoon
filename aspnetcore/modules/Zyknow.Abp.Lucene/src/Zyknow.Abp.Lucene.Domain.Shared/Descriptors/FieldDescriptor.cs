using System.Globalization;
using System.Linq.Expressions;

namespace Zyknow.Abp.Lucene.Descriptors;

public class FieldDescriptor
{
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
    public List<string> Depends { get; } = [];
    public bool Searchable { get; set; } = true;
    public LuceneNumericKind NumericKind { get; set; } = LuceneNumericKind.None;

    internal static string InferName(LambdaExpression exp)
    {
        if (exp.Body is MemberExpression m)
        {
            return m.Member.Name;
        }

        if (exp.Body is UnaryExpression u && u.Operand is MemberExpression um)
        {
            return um.Member.Name;
        }

        return "Field";
    }
}