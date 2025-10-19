using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Zyknow.Abp.Lucene.Dtos;

namespace Zyknow.Abp.Lucene.Controllers;

[Area("lucene")]
[Route("api/lucene")]
public class LuceneController : AbpControllerBase
{
    private readonly ILuceneService _service;

    public LuceneController(ILuceneService service)
    {
        _service = service;
    }

    [HttpPost("search/{entity}")]
    public Task<SearchResultDto> SearchAsync([FromRoute] string entity, [FromBody] SearchQueryInput input)
    {
        return _service.SearchAsync(entity, input);
    }

    [HttpPost("rebuild/{entity}")]
    public Task<int> RebuildIndexAsync([FromRoute] string entity)
    {
        return _service.RebuildIndexAsync(entity);
    }

    [HttpPost("rebuild-and-index/{entity}")]
    public Task<int> RebuildAndIndexAllAsync([FromRoute] string entity, [FromQuery] int batchSize = 1000)
    {
        return _service.RebuildAndIndexAllAsync(entity, batchSize);
    }

    [HttpGet("count/{entity}")]
    public Task<int> GetIndexDocumentCountAsync([FromRoute] string entity)
    {
        return _service.GetIndexDocumentCountAsync(entity);
    }

    [HttpGet("dump/{entity}")]
    public Task<SearchResultDto> DumpIndexAsync([FromRoute] string entity, [FromQuery] int take = 10)
    {
        return _service.DumpIndexAsync(entity, take);
    }
}