using Vctoon.Identities;
using Vctoon.Mediums.Dtos;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Vctoon.Mediums;

public partial class MediumAppService
{
    protected override async Task<IQueryable<Medium>> CreateFilteredQueryAsync(MediumGetListInput input)
    {
        var query = await mediumRepository.WithUserPageDetailsAsync(CurrentUser.Id);

        // TODO: AbpHelper generated
        query = query
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(),
                x => x.Title.Contains(input.Filter!) || x.Description.Contains(input.Filter!))
            .WhereIf(input.LibraryId != null, x => x.LibraryId == input.LibraryId)
            .WhereIf(!input.Artists.IsNullOrEmpty(), x => x.Artists.Any(a => input.Artists!.Contains(a.Id)))
            .WhereIf(!input.Tags.IsNullOrEmpty(), x => x.Tags.Any(t => input.Tags!.Contains(t.Id)));

        // Plex 风格混排：同一个列表里同时显示 Series 与普通 Medium
        if (!input.IncludeSeries && !input.IncludeMediums)
        {
            query = query.Where(_ => false);
        }
        else if (!input.IncludeSeries)
        {
            query = query.Where(x => !x.IsSeries);
        }
        else if (!input.IncludeMediums)
        {
            query = query.Where(x => x.IsSeries);
        }

        if (CurrentUser.Id != null)
        {
            var userId = CurrentUser.Id.Value;
            if (input.ReadingProgressType != null)
            {
                query = input.ReadingProgressType switch
                {
                    ReadingProgressType.NotStarted => query.Where(x =>
                        !x.Processes.AsQueryable().Any(p => p.UserId == userId && p.Progress > 0)
                    ),
                    ReadingProgressType.InProgress => query.Where(x => x.Processes.AsQueryable()
                        .Any(p => p.UserId == userId && p.Progress > 0 && p.Progress < 1)),
                    ReadingProgressType.Completed => query.Where(x => x.Processes.AsQueryable()
                        .Any(p => p.UserId == userId && p.Progress >= 1)),
                    _ => query
                };
            }
        }

        if (input.CreatedInDays is > 0)
        {
            var from = Clock.Now.AddDays(-input.CreatedInDays.Value);
            query = query.Where(x => x.CreationTime >= from);
        }

        if (CurrentUser.Id != null)
        {
            query = query.WhereIf(input.HasReadCount == true, x => x.ReadCount > 0);
            query = query.WhereIf(input.HasReadCount == false, x => x.ReadCount == 0);
        }

        return query;
    }

    protected virtual IQueryable<Medium> ApplyMediumAddendSorting(IQueryable<Medium> query, MediumGetListInput input,
        out bool hasSorting)
    {
        hasSorting = false;
        if (input.Sorting.IsNullOrWhiteSpace())
        {
            return query;
        }

        if (CurrentUser.Id == null)
        {
            return query;
        }

        var field = input.Sorting.Split(' ')[0];
        var isAsc = !input.Sorting.EndsWith(" desc", StringComparison.OrdinalIgnoreCase);

        if (field.Equals(nameof(MediumGetListOutputDto.ReadingLastTime), StringComparison.OrdinalIgnoreCase))
        {
            hasSorting = true;
            var userId = CurrentUser.Id!.Value;
            return isAsc
                ? query.OrderBy(x => x.Processes.Where(p => p.UserId == userId).Max(p => p.LastReadTime))
                : query.OrderByDescending(x =>
                    x.Processes.Where(p => p.UserId == userId).Max(p => p.LastReadTime));
        }

        if (field.Equals(nameof(MediumGetListOutputDto.ReadingProgress), StringComparison.OrdinalIgnoreCase))
        {
            hasSorting = true;
            var userId = CurrentUser.Id!.Value;
            return isAsc
                ? query.OrderBy(x => x.Processes.Where(p => p.UserId == userId)
                    .Select(p => (double?)p.Progress).Max() ?? 0)
                : query.OrderByDescending(x => x.Processes.Where(p => p.UserId == userId)
                    .Select(p => (double?)p.Progress).Max() ?? 0);
        }

        return query;
    }

    protected override IQueryable<Medium> ApplySorting(IQueryable<Medium> query, MediumGetListInput input)
    {
        var sorted = ApplyMediumAddendSorting(query, input, out var hasSorting);
        if (hasSorting)
        {
            return sorted;
        }

        return base.ApplySorting(query, input);
    }

    protected override async Task<Medium> GetEntityByIdAsync(Guid id)
    {
        return (await mediumRepository.WithUserDetailsAsync(CurrentUser.Id))
                   .FirstOrDefault(x => x.Id == id)
               ?? throw new EntityNotFoundException(typeof(Medium), id);
    }
}
