using Zyknow.Abp.Lucene.Dtos;

namespace Zyknow.Abp.Lucene;

public interface ILuceneService
{
    Task<SearchResultDto> SearchAsync(string entityName, SearchQueryInput input);
    Task<SearchResultDto> SearchManyAsync(MultiSearchInput input);
    Task<int> RebuildIndexAsync(string entityName);
    Task<int> RebuildAndIndexAllAsync(string entityName, int batchSize = 1000);
    Task<int> GetIndexDocumentCountAsync(string entityName);
}