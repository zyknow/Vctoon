using System.Threading.Tasks;
using System.Linq.Expressions;
using Lucene.Net.Search;
using Zyknow.Abp.Lucene.Descriptors;
using Zyknow.Abp.Lucene.Dtos;

namespace Zyknow.Abp.Lucene.Filtering;

public sealed record SearchFilterContext(
    string EntityName,
    EntitySearchDescriptor Descriptor,
    SearchQueryInput Input
)
{
    // 可选：使用端可直接提供 LINQ 表达式，由管线转换为 Lucene Query
    public LambdaExpression? Expression { get; init; }
}

public interface ILuceneFilterProvider
{
    Task<Query?> BuildAsync(SearchFilterContext ctx);
}