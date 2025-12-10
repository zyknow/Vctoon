using Vctoon.Mediums.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace Vctoon.Mediums;

public partial class MediumAppService
{
    public override async Task<MediumDto> GetAsync(Guid id)
    {
        var dto = await base.GetAsync(id);
        if (dto.IsSeries)
        {
            await FillSeriesCountAsync(new[] { dto });
        }
        return dto;
    }

    public override async Task<PagedResultDto<MediumDto>> GetListAsync(MediumGetListInput input)
    {
        var result = await base.GetListAsync(input);

        // 只有当列表可能包含 Series 时，才额外做一次批量统计。
        if (input.IncludeSeries && result.Items.Any(x => x.IsSeries))
        {
            await FillSeriesCountAsync(result.Items);
        }
        return result;
    }

    public virtual async Task<List<MediumDto>> GetSeriesListAsync(Guid mediumId)
    {
        await CheckGetListPolicyAsync();

        var series = await GetEntityByIdAsync(mediumId);
        if (!series.IsSeries)
        {
            throw new UserFriendlyException("目标 Medium 不是 Series");
        }

        var linkQuery = await mediumSeriesLinkRepository.GetQueryableAsync();
        var mediumQuery = await mediumRepository.WithUserPageDetailsAsync(CurrentUser.Id);

        var query =
            from link in linkQuery
            join m in mediumQuery on link.MediumId equals m.Id
            where link.SeriesId == mediumId
            orderby link.Sort, m.Title
            select m;

        var list = await AsyncExecuter.ToListAsync(query);
        var dtos = ObjectMapper.Map<List<Medium>, List<MediumDto>>(list);
        await FillSeriesCountAsync(dtos);
        return dtos;
    }

    public virtual async Task UpdateSeriesSortAsync(MediumSeriesSortUpdateDto input)
    {
        await CheckUpdatePolicyAsync();

        if (input.SeriesId == Guid.Empty)
        {
            throw new UserFriendlyException("SeriesId 不能为空");
        }

        var series = await GetEntityByIdAsync(input.SeriesId);
        if (!series.IsSeries)
        {
            throw new UserFriendlyException("目标 Medium 不是 Series");
        }

        var mediumIds = (input.MediumIds ?? new List<Guid>())
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (mediumIds.Count == 0)
        {
            throw new UserFriendlyException("MediumIds 不能为空");
        }

        var linkQuery = await mediumSeriesLinkRepository.GetQueryableAsync();

        // 为避免“只提交部分条目导致排序混乱”，要求提交该系列的完整条目列表。
        var totalCount = await AsyncExecuter.CountAsync(linkQuery.Where(l => l.SeriesId == input.SeriesId));
        if (totalCount != mediumIds.Count)
        {
            throw new UserFriendlyException("必须提交该系列的完整条目列表");
        }

        var links = await AsyncExecuter.ToListAsync(
            linkQuery.Where(l => l.SeriesId == input.SeriesId && mediumIds.Contains(l.MediumId))
        );

        if (links.Count != mediumIds.Count)
        {
            throw new UserFriendlyException("MediumIds 包含不属于该系列的条目");
        }

        var sortMap = mediumIds
            .Select((id, index) => new { id, index })
            .ToDictionary(x => x.id, x => x.index);

        foreach (var link in links)
        {
            link.Sort = sortMap[link.MediumId];
        }

        await mediumSeriesLinkRepository.UpdateManyAsync(links, true);
    }

    protected virtual async Task FillSeriesCountAsync(IReadOnlyList<MediumDto> items)
    {
        var seriesIds = items
            .Where(x => x.IsSeries)
            .Select(x => x.Id)
            .Distinct()
            .ToList();

        if (seriesIds.Count == 0)
        {
            return;
        }

        var linkQuery = await mediumSeriesLinkRepository.GetQueryableAsync();

        var counts = await AsyncExecuter.ToListAsync(
            linkQuery
                .Where(l => seriesIds.Contains(l.SeriesId))
                .GroupBy(l => l.SeriesId)
                .Select(g => new { SeriesId = g.Key, Count = g.Count() })
        );

        var dict = counts.ToDictionary(x => x.SeriesId, x => x.Count);

        foreach (var item in items)
        {
            if (!item.IsSeries)
            {
                item.SeriesCount = null;
                continue;
            }

            item.SeriesCount = dict.TryGetValue(item.Id, out var value) ? value : 0;
        }
    }
}
