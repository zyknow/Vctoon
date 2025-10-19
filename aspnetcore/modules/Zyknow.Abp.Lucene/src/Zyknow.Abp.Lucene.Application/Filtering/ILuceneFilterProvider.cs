using Lucene.Net.Search;

namespace Zyknow.Abp.Lucene.Filtering;

public interface ILuceneFilterProvider
{
    Task<Query?> BuildAsync(SearchFilterContext ctx);
}