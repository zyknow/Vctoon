using System.Linq.Expressions;
using Vctoon.Identities;

namespace Vctoon.Extensions;

/// <summary>
/// 针对属性选择器表达式的布尔谓词扩展，便于以 LINQ 风格生成可由 EF 翻译的条件表达式。
/// </summary>
public static class ExpressionPredicateExtensions
{
    // 泛型版本：强类型属性选择器 => 布尔谓词（NotNull/IsNull/Equal/NotEqual）
    public static Expression<Func<T, bool>> NotNull<T, TProp>(this Expression<Func<T, TProp>> selector)
    {
        var param = selector.Parameters[0];
        var body = selector.Body;
        var propType = body.Type;

        BinaryExpression test;
        var underlying = Nullable.GetUnderlyingType(propType);
        if (underlying != null || !propType.IsValueType)
        {
            test = Expression.NotEqual(body, Expression.Constant(null, propType));
        }
        else
        {
            test = Expression.NotEqual(body, Expression.Default(propType));
        }

        return Expression.Lambda<Func<T, bool>>(test, param);
    }

    public static Expression<Func<T, bool>> IsNull<T, TProp>(this Expression<Func<T, TProp>> selector)
    {
        var param = selector.Parameters[0];
        var body = selector.Body;
        var propType = body.Type;

        BinaryExpression test;
        var underlying = Nullable.GetUnderlyingType(propType);
        if (underlying != null || !propType.IsValueType)
        {
            test = Expression.Equal(body, Expression.Constant(null, propType));
        }
        else
        {
            test = Expression.Equal(body, Expression.Default(propType));
        }

        return Expression.Lambda<Func<T, bool>>(test, param);
    }

    public static Expression<Func<T, bool>> EqualTo<T, TProp>(this Expression<Func<T, TProp>> selector, TProp value)
    {
        var param = selector.Parameters[0];
        var body = selector.Body;
        var constVal = Expression.Constant(value, body.Type);
        var test = Expression.Equal(body, constVal);
        return Expression.Lambda<Func<T, bool>>(test, param);
    }

    public static Expression<Func<T, bool>> NotEqualTo<T, TProp>(this Expression<Func<T, TProp>> selector, TProp value)
    {
        var param = selector.Parameters[0];
        var body = selector.Body;
        var constVal = Expression.Constant(value, body.Type);
        var test = Expression.NotEqual(body, constVal);
        return Expression.Lambda<Func<T, bool>>(test, param);
    }

    // 非泛型入口：用于当前基类属性 LambdaExpression（必须来自 IdentityUserReadingProcess）
    public static Expression<Func<IdentityUserReadingProcess, bool>> NotNull(this LambdaExpression selector)
    {
        if (selector is null) throw new ArgumentNullException(nameof(selector));
        if (selector.Parameters.Count != 1 || selector.Parameters[0].Type != typeof(IdentityUserReadingProcess))
        {
            throw new InvalidOperationException("Selector must be (IdentityUserReadingProcess p) => p.Property");
        }

        var param = (ParameterExpression)selector.Parameters[0];
        var body = selector.Body;
        var propType = body.Type;

        BinaryExpression test;
        var underlying = Nullable.GetUnderlyingType(propType);
        if (underlying != null || !propType.IsValueType)
        {
            test = Expression.NotEqual(body, Expression.Constant(null, propType));
        }
        else
        {
            test = Expression.NotEqual(body, Expression.Default(propType));
        }

        return Expression.Lambda<Func<IdentityUserReadingProcess, bool>>(test, param);
    }

    public static Expression<Func<IdentityUserReadingProcess, bool>> IsNull(this LambdaExpression selector)
    {
        if (selector is null) throw new ArgumentNullException(nameof(selector));
        if (selector.Parameters.Count != 1 || selector.Parameters[0].Type != typeof(IdentityUserReadingProcess))
        {
            throw new InvalidOperationException("Selector must be (IdentityUserReadingProcess p) => p.Property");
        }

        var param = (ParameterExpression)selector.Parameters[0];
        var body = selector.Body;
        var propType = body.Type;

        BinaryExpression test;
        var underlying = Nullable.GetUnderlyingType(propType);
        if (underlying != null || !propType.IsValueType)
        {
            test = Expression.Equal(body, Expression.Constant(null, propType));
        }
        else
        {
            test = Expression.Equal(body, Expression.Default(propType));
        }

        return Expression.Lambda<Func<IdentityUserReadingProcess, bool>>(test, param);
    }
}

