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

    public virtual async Task<List<MediumGetListOutputDto>> GetSeriesListAsync(Guid mediumId)
    {
        await CheckGetListPolicyAsync();

        var series = await GetEntityByIdAsync(mediumId);
        if (!series.IsSeries)
        {
            throw new UserFriendlyException(L["TargetMediumIsNotSeries"]);
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
        var dtos = ObjectMapper.Map<List<Medium>, List<MediumGetListOutputDto>>(list);
        return dtos;
    }

    public virtual async Task UpdateSeriesSortAsync(MediumSeriesSortUpdateDto input)
    {
        await CheckUpdatePolicyAsync();

        if (input.SeriesId == Guid.Empty)
        {
            throw new UserFriendlyException(L["SeriesIdCannotBeEmpty"]);
        }

        var series = await GetEntityByIdAsync(input.SeriesId);
        if (!series.IsSeries)
        {
            throw new UserFriendlyException(L["TargetMediumIsNotSeries"]);
        }

        var mediumIds = (input.MediumIds ?? new List<Guid>())
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (mediumIds.Count == 0)
        {
            throw new UserFriendlyException(L["MediumIdsCannotBeEmpty"]);
        }

        var linkQuery = await mediumSeriesLinkRepository.GetQueryableAsync();

        // 为避免“只提交部分条目导致排序混乱”，要求提交该系列的完整条目列表。
        var totalCount = await AsyncExecuter.CountAsync(linkQuery.Where(l => l.SeriesId == input.SeriesId));
        if (totalCount != mediumIds.Count)
        {
            throw new UserFriendlyException(L["MustSubmitCompleteSeriesItemList"]);
        }

        var links = await AsyncExecuter.ToListAsync(
            linkQuery.Where(l => l.SeriesId == input.SeriesId && mediumIds.Contains(l.MediumId))
        );

        if (links.Count != mediumIds.Count)
        {
            throw new UserFriendlyException(L["MediumIdsContainsItemsNotBelongingToSeries"]);
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

    public virtual async Task AddSeriesMediumListAsync(MediumSeriesListUpdateDto input)
    {
        await CheckUpdatePolicyAsync();

        if (input.SeriesId == Guid.Empty)
        {
            throw new UserFriendlyException(L["SeriesIdCannotBeEmpty"]);
        }

        var series = await GetEntityByIdAsync(input.SeriesId);
        if (!series.IsSeries)
        {
            throw new UserFriendlyException(L["TargetMediumIsNotSeries"]);
        }

        if (series.Cover.IsNullOrWhiteSpace())
        {
            // if series has no cover, set the first medium's cover as series cover
            var firstMediumId = input.MediumIds?.FirstOrDefault(x => x != Guid.Empty);
            if (firstMediumId.HasValue && firstMediumId != Guid.Empty)
            {
                var firstMedium = await mediumRepository.GetAsync(firstMediumId.Value);
                series.Cover = firstMedium.Cover;
                await mediumRepository.UpdateAsync(series);
            }
        }

        var mediumIds = (input.MediumIds ?? new List<Guid>())
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (mediumIds.Count == 0)
        {
            return;
        }

        var linkQuery = await mediumSeriesLinkRepository.GetQueryableAsync();

        // 过滤掉已存在的
        var existingMediumIds = await AsyncExecuter.ToListAsync(
            linkQuery
                .Where(l => l.SeriesId == input.SeriesId && mediumIds.Contains(l.MediumId))
                .Select(l => l.MediumId)
        );

        var newMediumIds = mediumIds.Except(existingMediumIds).ToList();
        if (newMediumIds.Count == 0)
        {
            return;
        }

        // 检查这些 Medium 是否存在
        var mediumsToAdd = await mediumRepository.GetListAsync(x => newMediumIds.Contains(x.Id));

        // 获取当前最大的 Sort
        var maxSort = 0;
        var hasAny = await AsyncExecuter.AnyAsync(linkQuery.Where(l => l.SeriesId == input.SeriesId));
        if (hasAny)
        {
            maxSort = await AsyncExecuter.MaxAsync(
               linkQuery.Where(l => l.SeriesId == input.SeriesId),
               l => l.Sort
           );
        }

        var linksToInsert = new List<MediumSeriesLink>();
        foreach (var medium in mediumsToAdd)
        {
            // 防止自引用
            if (medium.Id == input.SeriesId) continue;

            maxSort++;
            linksToInsert.Add(new MediumSeriesLink(GuidGenerator.Create(), input.SeriesId, medium.Id, maxSort));
        }

        if (linksToInsert.Count > 0)
        {
            await mediumSeriesLinkRepository.InsertManyAsync(linksToInsert, true);
        }
    }

    public virtual async Task DeleteSeriesMediumListAsync(MediumSeriesListUpdateDto input)
    {
        await CheckUpdatePolicyAsync();

        if (input.SeriesId == Guid.Empty)
        {
            throw new UserFriendlyException(L["SeriesIdCannotBeEmpty"]);
        }

        var mediumIds = (input.MediumIds ?? new List<Guid>())
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (mediumIds.Count == 0)
        {
            return;
        }

        var linkQuery = await mediumSeriesLinkRepository.GetQueryableAsync();
        var links = await AsyncExecuter.ToListAsync(
            linkQuery.Where(l => l.SeriesId == input.SeriesId && mediumIds.Contains(l.MediumId))
        );

        if (links.Count > 0)
        {
            await mediumSeriesLinkRepository.DeleteManyAsync(links, true);
        }
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
