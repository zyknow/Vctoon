using System.Collections;
using System.Linq.Expressions;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Zyknow.Abp.Lucene.Descriptors;

namespace Zyknow.Abp.Lucene.Filtering;

public static class LinqLucene
{
    public static Query Where<T>(EntitySearchDescriptor descriptor, Expression<Func<T, bool>> expr)
    {
        return Visit(descriptor, expr.Body);
    }

    private static Query Visit(EntitySearchDescriptor descriptor, Expression node)
    {
        switch (node)
        {
            case BinaryExpression be when be.NodeType == ExpressionType.Equal:
            {
                var fieldName = ResolveMemberName(descriptor, be.Left is MemberExpression ? be.Left : be.Right);
                var fd = descriptor.Fields.FirstOrDefault(f => f.Name == fieldName);
                if (fd != null && fd.NumericKind != LuceneNumericKind.None)
                {
                    var (_, obj) = ExtractEqObject(descriptor, be);
                    return BuildNumericEquality(fd, obj);
                }

                var (eqFieldName, value) = ExtractEq(descriptor, be);
                value = NormalizeForKeyword(descriptor, eqFieldName, value);
                return new TermQuery(new Term(eqFieldName, value));
            }

            case BinaryExpression be when be.NodeType == ExpressionType.AndAlso:
            {
                var left = Visit(descriptor, be.Left);
                var right = Visit(descriptor, be.Right);
                var bq = new BooleanQuery { { left, Occur.MUST }, { right, Occur.MUST } };
                return bq;
            }

            case BinaryExpression be when be.NodeType == ExpressionType.OrElse:
            {
                var left = Visit(descriptor, be.Left);
                var right = Visit(descriptor, be.Right);
                var bq = new BooleanQuery { { left, Occur.SHOULD }, { right, Occur.SHOULD } };
                bq.MinimumNumberShouldMatch = 1;
                return bq;
            }

            case BinaryExpression be when IsRangeComparison(be.NodeType):
            {
                // Numeric-aware range
                var fieldName = ResolveMemberName(descriptor, be.Left is MemberExpression ? be.Left : be.Right);
                var field = descriptor.Fields.FirstOrDefault(f => f.Name == fieldName);
                if (field != null && field.NumericKind != LuceneNumericKind.None)
                {
                    return BuildNumericRangeQuery(field, be);
                }

                return BuildTextRangeQuery(descriptor, be);
            }

            case MethodCallExpression mc:
                return VisitMethodCall(descriptor, mc);
        }

        throw new NotSupportedException($"Unsupported expression node: {node.NodeType}");
    }

    private static bool IsRangeComparison(ExpressionType type)
    {
        return type == ExpressionType.GreaterThan || type == ExpressionType.LessThan ||
               type == ExpressionType.GreaterThanOrEqual || type == ExpressionType.LessThanOrEqual;
    }

    private static Query BuildTextRangeQuery(EntitySearchDescriptor descriptor, BinaryExpression be)
    {
        if (!TryExtractMemberAndConstant(descriptor, be.Left, be.Right, out var fieldName, out var constValue)
            && !TryExtractMemberAndConstant(descriptor, be.Right, be.Left, out fieldName, out constValue))
        {
            throw new NotSupportedException("Range comparison must compare field to a constant value.");
        }

        constValue = NormalizeForKeyword(descriptor, fieldName, constValue);
        var includeLower = be.NodeType == ExpressionType.GreaterThanOrEqual;
        var includeUpper = be.NodeType == ExpressionType.LessThanOrEqual;
        string? lowerTerm = null, upperTerm = null;
        switch (be.NodeType)
        {
            case ExpressionType.GreaterThan:
            case ExpressionType.GreaterThanOrEqual:
                lowerTerm = constValue;
                upperTerm = null;
                break;
            case ExpressionType.LessThan:
            case ExpressionType.LessThanOrEqual:
                lowerTerm = null;
                upperTerm = constValue;
                break;
        }

        var lower = lowerTerm != null ? new BytesRef(lowerTerm) : null;
        var upper = upperTerm != null ? new BytesRef(upperTerm) : null;
        return new TermRangeQuery(fieldName, lower, upper, includeLower, includeUpper);
    }

    private static Query VisitMethodCall(EntitySearchDescriptor descriptor, MethodCallExpression mc)
    {
        // Enumerable.Contains(collection, member)
        if (mc.Method.DeclaringType == typeof(Enumerable) && mc.Method.Name == nameof(Enumerable.Contains))
        {
            if (mc.Arguments.Count == 2)
            {
                var collection = Evaluate(mc.Arguments[0]);
                var member = mc.Arguments[1];
                var fieldName = ResolveMemberName(descriptor, member);
                var bq = new BooleanQuery();
                foreach (var item in AsEnumerable(collection))
                {
                    var s = item?.ToString();
                    if (!string.IsNullOrEmpty(s))
                    {
                        var normalized = NormalizeForKeyword(descriptor, fieldName, s);
                        bq.Add(new TermQuery(new Term(fieldName, normalized)), Occur.SHOULD);
                    }
                }

                bq.MinimumNumberShouldMatch = 1;
                return bq;
            }
        }

        // Instance .Contains: collection.Contains(member)
        if (mc.Method.Name == nameof(Enumerable.Contains) && mc.Arguments.Count == 1)
        {
            var collection = Evaluate(mc.Object);
            var member = mc.Arguments[0];
            var fieldName = ResolveMemberName(descriptor, member);
            var bq = new BooleanQuery();
            foreach (var item in AsEnumerable(collection))
            {
                var s = item?.ToString();
                if (!string.IsNullOrEmpty(s))
                {
                    var normalized = NormalizeForKeyword(descriptor, fieldName, s);
                    bq.Add(new TermQuery(new Term(fieldName, normalized)), Occur.SHOULD);
                }
            }

            bq.MinimumNumberShouldMatch = 1;
            return bq;
        }

        // x.Field.StartsWith(prefix)
        if (mc.Method.DeclaringType == typeof(string) && mc.Method.Name == nameof(string.StartsWith) &&
            mc.Object is MemberExpression mem)
        {
            var fieldName = ResolveMemberName(descriptor, mem);
            var prefixObj = Evaluate(mc.Arguments[0]);
            var prefix = prefixObj?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(prefix))
            {
                throw new NotSupportedException("StartsWith requires a non-empty prefix.");
            }

            prefix = NormalizeForKeyword(descriptor, fieldName, prefix);
            return new PrefixQuery(new Term(fieldName, prefix));
        }

        // x.Field.EndsWith(suffix) -> 通配符查询 *suffix
        if (mc.Method.DeclaringType == typeof(string) && mc.Method.Name == nameof(string.EndsWith) &&
            mc.Object is MemberExpression mem2)
        {
            var fieldName = ResolveMemberName(descriptor, mem2);
            var suffixObj = Evaluate(mc.Arguments[0]);
            var suffix = suffixObj?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(suffix))
            {
                throw new NotSupportedException("EndsWith requires a non-empty suffix.");
            }

            suffix = NormalizeForKeyword(descriptor, fieldName, suffix);
            return new WildcardQuery(new Term(fieldName, "*" + suffix));
        }

        // x.Field.Contains(substring) -> 通配符 *substring*
        if (mc.Method.DeclaringType == typeof(string) && mc.Method.Name == nameof(string.Contains) &&
            mc.Object is MemberExpression mem3)
        {
            var fieldName = ResolveMemberName(descriptor, mem3);
            var subObj = Evaluate(mc.Arguments[0]);
            var sub = subObj?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(sub))
            {
                throw new NotSupportedException("Contains requires a non-empty substring.");
            }

            sub = NormalizeForKeyword(descriptor, fieldName, sub);
            return new WildcardQuery(new Term(fieldName, "*" + sub + "*"));
        }

        throw new NotSupportedException($"Unsupported method call: {mc.Method.DeclaringType?.Name}.{mc.Method.Name}");
    }

    private static string NormalizeForKeyword(EntitySearchDescriptor descriptor, string fieldName, string value)
    {
        var field = descriptor.Fields.FirstOrDefault(f => string.Equals(f.Name, fieldName, StringComparison.Ordinal));
        if (field is null)
        {
            return value;
        }

        if (field.LowerCaseKeyword)
        {
            return field.LowerCaseCulture != null ? value.ToLower(field.LowerCaseCulture) : value.ToLowerInvariant();
        }

        return value;
    }

    private static (string fieldName, string value) ExtractEq(EntitySearchDescriptor descriptor, BinaryExpression be)
    {
        if (TryExtractMemberAndConstant(descriptor, be.Left, be.Right, out var field, out var value))
        {
            return (field, value);
        }

        if (TryExtractMemberAndConstant(descriptor, be.Right, be.Left, out field, out value))
        {
            return (field, value);
        }

        throw new NotSupportedException("Equality must compare field to a constant value.");
    }

    private static (string fieldName, object? value) ExtractEqObject(EntitySearchDescriptor descriptor,
        BinaryExpression be)
    {
        if (TryExtractMemberAndObject(descriptor, be.Left, be.Right, out var field, out var value))
        {
            return (field, value);
        }

        if (TryExtractMemberAndObject(descriptor, be.Right, be.Left, out field, out value))
        {
            return (field, value);
        }

        throw new NotSupportedException("Equality must compare field to a constant value.");
    }

    private static bool TryExtractMemberAndConstant(EntitySearchDescriptor descriptor, Expression memberExpr,
        Expression constExpr, out string fieldName, out string value)
    {
        fieldName = ResolveMemberName(descriptor, memberExpr);
        var obj = Evaluate(constExpr);
        value = obj?.ToString() ?? string.Empty;
        return !string.IsNullOrEmpty(fieldName) && !string.IsNullOrEmpty(value);
    }

    private static bool TryExtractMemberAndObject(EntitySearchDescriptor descriptor, Expression memberExpr,
        Expression constExpr, out string fieldName, out object? value)
    {
        fieldName = ResolveMemberName(descriptor, memberExpr);
        value = Evaluate(constExpr);
        return !string.IsNullOrEmpty(fieldName) && value != null;
    }

    private static string ResolveMemberName(EntitySearchDescriptor descriptor, Expression exp)
    {
        if (exp is MemberExpression m)
        {
            var name = m.Member.Name;
            var field = descriptor.Fields.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.Ordinal));
            if (field == null)
            {
                throw new InvalidOperationException(
                    $"Field '{name}' not found in descriptor for {descriptor.EntityType.Name}.");
            }

            return field.Name;
        }

        if (exp is UnaryExpression u && u.Operand is MemberExpression um)
        {
            var name = um.Member.Name;
            var field = descriptor.Fields.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.Ordinal));
            if (field == null)
            {
                throw new InvalidOperationException(
                    $"Field '{name}' not found in descriptor for {descriptor.EntityType.Name}.");
            }

            return field.Name;
        }

        throw new NotSupportedException("Only member expressions are supported as field selectors.");
    }

    private static object? Evaluate(Expression exp)
    {
        if (exp is ConstantExpression c)
        {
            return c.Value;
        }

        var lambda = Expression.Lambda(exp);
        var compiled = lambda.Compile();
        return compiled.DynamicInvoke();
    }

    private static IEnumerable AsEnumerable(object? obj)
    {
        if (obj is IEnumerable e)
        {
            return e;
        }

        throw new NotSupportedException("Contains requires an enumerable collection.");
    }

    private static Query BuildNumericRangeQuery(FieldDescriptor field, BinaryExpression be)
    {
        var includeLower = be.NodeType == ExpressionType.GreaterThanOrEqual;
        var includeUpper = be.NodeType == ExpressionType.LessThanOrEqual;
        object? lowerObj = null, upperObj = null;
        switch (be.NodeType)
        {
            case ExpressionType.GreaterThan:
            case ExpressionType.GreaterThanOrEqual:
                lowerObj = Evaluate(be.Right);
                break;
            case ExpressionType.LessThan:
            case ExpressionType.LessThanOrEqual:
                upperObj = Evaluate(be.Right);
                break;
        }

        return BuildNumericRange(field, lowerObj, upperObj, includeLower, includeUpper);
    }

    private static Query BuildNumericEquality(FieldDescriptor field, object? obj)
    {
        return BuildNumericRange(field, obj, obj, true, true);
    }

    private static Query BuildNumericRange(FieldDescriptor field, object? lowerObj, object? upperObj, bool includeLower,
        bool includeUpper)
    {
        switch (field.NumericKind)
        {
            case LuceneNumericKind.Int32:
            {
                var min = lowerObj != null ? Convert.ToInt32(lowerObj) : (int?)null;
                var max = upperObj != null ? Convert.ToInt32(upperObj) : (int?)null;
                return NumericRangeQuery.NewInt32Range(field.Name, min, max, includeLower, includeUpper);
            }
            case LuceneNumericKind.Int64:
            {
                var min = lowerObj != null ? Convert.ToInt64(lowerObj) : (long?)null;
                var max = upperObj != null ? Convert.ToInt64(upperObj) : (long?)null;
                return NumericRangeQuery.NewInt64Range(field.Name, min, max, includeLower, includeUpper);
            }
            case LuceneNumericKind.DateEpochMillis:
            case LuceneNumericKind.DateEpochSeconds:
            {
                var min = lowerObj != null ? ToEpoch(field.NumericKind, lowerObj) : (long?)null;
                var max = upperObj != null ? ToEpoch(field.NumericKind, upperObj) : (long?)null;
                return NumericRangeQuery.NewInt64Range(field.Name, min, max, includeLower, includeUpper);
            }
            default:
                throw new NotSupportedException($"Unsupported numeric kind: {field.NumericKind}");
        }
    }

    private static long ToEpoch(LuceneNumericKind kind, object value)
    {
        if (value is DateTime dt)
        {
            var dto = new DateTimeOffset(dt);
            return kind == LuceneNumericKind.DateEpochMillis ? dto.ToUnixTimeMilliseconds() : dto.ToUnixTimeSeconds();
        }

        if (value is long l)
        {
            return l;
        }

        var parsed = DateTime.Parse(value.ToString()!);
        var dto2 = new DateTimeOffset(parsed);
        return kind == LuceneNumericKind.DateEpochMillis ? dto2.ToUnixTimeMilliseconds() : dto2.ToUnixTimeSeconds();
    }
}