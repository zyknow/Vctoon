using Volo.Abp.Application.Dtos;

namespace Zyknow.Abp.Lucene.Dtos;

/// <summary>
/// 搜索结果的分页响应。
/// - <see cref="TotalCount"/>：总命中数（TopDocs.TotalHits）。
/// - <see cref="Items"/>：当前页的命中项集合。
/// - 支持 ABP 标准的分页/排序模型，与 <see cref="SearchQueryInput"/> 配合使用。
/// </summary>
public class SearchResultDto : PagedResultDto<SearchHitDto>
{
    /// <summary>
    /// 初始化一个空的搜索结果（TotalCount=0，Items=空集合）。
    /// </summary>
    public SearchResultDto() : base(0, new List<SearchHitDto>())
    {
    }

    /// <summary>
    /// 使用总命中数与命中项集合初始化搜索结果。
    /// </summary>
    /// <param name="totalCount">总命中数。</param>
    /// <param name="items">当前页命中项集合。</param>
    public SearchResultDto(long totalCount, List<SearchHitDto> items) : base(totalCount, items)
    {
    }
}