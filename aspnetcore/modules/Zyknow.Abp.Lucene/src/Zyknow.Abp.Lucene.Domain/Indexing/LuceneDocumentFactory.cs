using System.Reflection;
using Lucene.Net.Documents;
using Zyknow.Abp.Lucene.Descriptors;

namespace Zyknow.Abp.Lucene.Indexing;

public static class LuceneDocumentFactory
{
    public static Document CreateDocument(object entity, EntitySearchDescriptor descriptor)
    {
        var doc = new Document();
        // ID field
        var idProp = entity.GetType().GetProperty(descriptor.IdFieldName, BindingFlags.Public | BindingFlags.Instance);
        var idVal = idProp?.GetValue(entity)?.ToString() ?? string.Empty;
        doc.Add(new StringField(descriptor.IdFieldName, idVal, Field.Store.YES));

        foreach (var field in descriptor.Fields)
        {
            var valObj = ResolveObjectValue(entity, field);
            if (valObj == null)
            {
                continue;
            }

            Field luceneField;
            if (!field.Searchable)
            {
                luceneField = new StoredField(field.Name, valObj.ToString());
            }
            else if (field.NumericKind != LuceneNumericKind.None)
            {
                luceneField = CreateNumericField(field, valObj);
            }
            else if (field.Keyword)
            {
                var strVal = valObj.ToString() ?? string.Empty;
                var writeValue = field.LowerCaseKeyword
                    ? field.LowerCaseCulture != null
                        ? strVal.ToLower(field.LowerCaseCulture)
                        : strVal.ToLowerInvariant()
                    : strVal;
                luceneField = new StringField(field.Name, writeValue, field.Store ? Field.Store.YES : Field.Store.NO);
            }
            else
            {
                luceneField = new TextField(field.Name, valObj.ToString(),
                    field.Store ? Field.Store.YES : Field.Store.NO);
            }

#pragma warning disable 618
            if (field.Boost is > 1.0f or < 1.0f)
            {
                luceneField.Boost = field.Boost;
            }
#pragma warning restore 618

            doc.Add(luceneField);
        }

        return doc;
    }

    public static Document CreateDocument(IReadOnlyDictionary<string, string> values, EntitySearchDescriptor descriptor)
    {
        var doc = new Document();
        // ID field from provided values
        values.TryGetValue(descriptor.IdFieldName, out var id);
        id ??= string.Empty;
        doc.Add(new StringField(descriptor.IdFieldName, id, Field.Store.YES));

        foreach (var field in descriptor.Fields)
        {
            if (!values.TryGetValue(field.Name, out var value) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            Field luceneField;
            if (!field.Searchable)
            {
                luceneField = new StoredField(field.Name, value);
            }
            else if (field.NumericKind != LuceneNumericKind.None)
            {
                luceneField = CreateNumericField(field, value);
            }
            else if (field.Keyword)
            {
                var writeValue = field.LowerCaseKeyword
                    ? field.LowerCaseCulture != null ? value.ToLower(field.LowerCaseCulture) : value.ToLowerInvariant()
                    : value;
                luceneField = new StringField(field.Name, writeValue, field.Store ? Field.Store.YES : Field.Store.NO);
            }
            else
            {
                luceneField = new TextField(field.Name, value, field.Store ? Field.Store.YES : Field.Store.NO);
            }

#pragma warning disable 618
            if (field.Boost is > 1.0f or < 1.0f)
            {
                luceneField.Boost = field.Boost;
            }
#pragma warning restore 618

            doc.Add(luceneField);
        }

        return doc;
    }

    private static object? ResolveObjectValue(object entity, FieldDescriptor field)
    {
        if (field.ValueSelector != null)
        {
            return field.ValueSelector.DynamicInvoke(entity);
        }

        if (field.Selector != null)
        {
            var compiled = field.Selector.Compile();
            var v = compiled.DynamicInvoke(entity);
            return v;
        }

        return null;
    }

    private static Field CreateNumericField(FieldDescriptor fd, object value)
    {
        switch (fd.NumericKind)
        {
            case LuceneNumericKind.Int32:
            {
                var iv = value is int i ? i : int.Parse(value.ToString()!);
                return new Int32Field(fd.Name, iv, fd.Store ? Field.Store.YES : Field.Store.NO);
            }
            case LuceneNumericKind.Int64:
            {
                var lv = value is long l ? l : long.Parse(value.ToString()!);
                return new Int64Field(fd.Name, lv, fd.Store ? Field.Store.YES : Field.Store.NO);
            }
            case LuceneNumericKind.DateEpochMillis:
            {
                long ms;
                if (value is DateTime dt)
                {
                    ms = new DateTimeOffset(dt).ToUnixTimeMilliseconds();
                }
                else if (value is long l)
                {
                    ms = l;
                }
                else
                {
                    ms = new DateTimeOffset(DateTime.Parse(value.ToString()!)).ToUnixTimeMilliseconds();
                }

                return new Int64Field(fd.Name, ms, fd.Store ? Field.Store.YES : Field.Store.NO);
            }
            case LuceneNumericKind.DateEpochSeconds:
            {
                long s;
                if (value is DateTime dt)
                {
                    s = new DateTimeOffset(dt).ToUnixTimeSeconds();
                }
                else if (value is long l)
                {
                    s = l;
                }
                else
                {
                    s = new DateTimeOffset(DateTime.Parse(value.ToString()!)).ToUnixTimeSeconds();
                }

                return new Int64Field(fd.Name, s, fd.Store ? Field.Store.YES : Field.Store.NO);
            }
            default:
                return new StoredField(fd.Name, value.ToString());
        }
    }
}