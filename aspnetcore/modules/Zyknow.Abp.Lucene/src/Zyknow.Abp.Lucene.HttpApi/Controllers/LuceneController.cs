using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Zyknow.Abp.Lucene.Dtos;

namespace Zyknow.Abp.Lucene.Controllers;

[Area("lucene")]
[Route("api/lucene")]
public class LuceneController(ILuceneService service) : AbpControllerBase
{
    [HttpGet("search/{entity}")]
    public Task<SearchResultDto> SearchAsync([FromRoute] string entity, [FromQuery] SearchQueryInput input)
    {
        return service.SearchAsync(entity, input);
    }

    [HttpGet("search-many")]
    public Task<SearchResultDto> SearchManyAsync([FromQuery] MultiSearchInput input)
    {
        return service.SearchManyAsync(input);
    }

    [HttpPost("rebuild/{entity}")]
    public Task<int> RebuildIndexAsync([FromRoute] string entity)
    {
        return service.RebuildIndexAsync(entity);
    }

    [HttpPost("rebuild-and-index/{entity}")]
    public Task<int> RebuildAndIndexAllAsync([FromRoute] string entity, [FromQuery] int batchSize = 1000)
    {
        return service.RebuildAndIndexAllAsync(entity, batchSize);
    }

    [HttpGet("count/{entity}")]
    public Task<int> GetIndexDocumentCountAsync([FromRoute] string entity)
    {
        return service.GetIndexDocumentCountAsync(entity);
    }
}