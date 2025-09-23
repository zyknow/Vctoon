using System.Linq.Expressions;
using Vctoon.Identities;
using Vctoon.Mediums.Dtos.Base;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Vctoon.Mediums.Base;

public abstract class MediumBaseAppService<TEntity, TGetOutputDto, TGetListOutputDto, TGetListInput, TCreateInput,
    TUpdateInput>(
    IRepository<TEntity, Guid> repository)
    : VctoonCrudAppService<TEntity, TGetOutputDto, TGetListOutputDto, Guid, TGetListInput, TCreateInput,
        TUpdateInput>(repository), IMediumBaseAppService<TGetOutputDto, TGetListOutputDto, TGetListInput, TCreateInput,
        TUpdateInput>
    where TEntity : MediumBase
    where TGetOutputDto : MediumDtoBase
    where TGetListOutputDto : MediumGetListOutputDtoBase
    where TGetListInput : MediumGetListInputBase
    where TCreateInput : MediumCreateUpdateDtoBase
    where TUpdateInput : MediumCreateUpdateDtoBase
{
    protected abstract Expression<Func<IdentityUserReadingProcess, bool>> ProcessPredicate { get; }

    protected override async Task<IQueryable<TEntity>> CreateFilteredQueryAsync(TGetListInput input)
    {
        // 修正：传入 Guid?，避免未登录时 CurrentUser.Id.Value 抛异常
        var query = await (Repository as IMediumBaseRepository<TEntity>)!.WithPageDetailsAsync(CurrentUser.Id);

        // TODO: AbpHelper generated
        query = query
                .WhereIf(!input.Title.IsNullOrWhiteSpace(), x => x.Title.Contains(input.Title!))
                .WhereIf(!input.Description.IsNullOrWhiteSpace(), x => x.Description.Contains(input.Description!))
                .WhereIf(input.LibraryId != null, x => x.LibraryId == input.LibraryId)
                .WhereIf(!input.Artists.IsNullOrEmpty(), x => x.Artists.Any(a => input.Artists!.Contains(a.Id)))
                .WhereIf(!input.Tags.IsNullOrEmpty(), x => x.Tags.Any(t => input.Tags!.Contains(t.Id)))
            ;

        if (CurrentUser.Id != null)
        {
            var userId = CurrentUser.Id.Value;
            if (input.HasReadingProgress != null)
            {
                var predicate = ProcessPredicate;
                query = query.Where(x => x.Processes.AsQueryable()
                    .Where(predicate)
                    .Any(p => p.UserId == userId && p.Progress > 0 && p.Progress < 1) == input.HasReadingProgress);
            }
        }

        if (input.CreatedInDays != null && input.CreatedInDays > 0)
        {
            var from = Clock.Now.AddDays(-input.CreatedInDays.Value);
            query = query.Where(x => x.CreationTime >= from);
        }

        if (CurrentUser.Id != null)
        {
            query = query.WhereIf(input.HasReadCount == true,
                x => x.ReadCount > 0);

            query = query.WhereIf(input.HasReadCount == false,
                x => x.ReadCount == 0);
        }


        return query;
    }


    protected virtual IQueryable<TEntity> ApplyMediumAddendSorting(IQueryable<TEntity> query, TGetListInput input,
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

        if (field.Equals(nameof(IMediumHasReadingProcessDto.ReadingLastTime), StringComparison.OrdinalIgnoreCase))
        {
            hasSorting = true;
            var userId = CurrentUser.Id!.Value;
            return isAsc
                ? query.OrderBy(x => x.Processes.Where(p => p.UserId == userId).Max(p => p.LastReadTime))
                : query.OrderByDescending(x =>
                    x.Processes.Where(p => p.UserId == userId).Max(p => p.LastReadTime));
        }
        else if (field.Equals(nameof(IMediumHasReadingProcessDto.ReadingProgress), StringComparison.OrdinalIgnoreCase))
        {
            hasSorting = true;
            var userId = CurrentUser.Id!.Value;
            return isAsc
                ? query.OrderBy(x =>
                    x.Processes.Where(p => p.UserId == userId).Select(p => (double?)p.Progress).Max() ?? 0)
                : query.OrderByDescending(x =>
                    x.Processes.Where(p => p.UserId == userId).Select(p => (double?)p.Progress).Max() ?? 0);
        }
        else
        {
            return query;
        }
    }

    // 新增：覆盖基类排序，拦截自定义字段，避免 Dynamic LINQ 再按字符串排序
    protected override IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, TGetListInput input)
    {
        var sorted = ApplyMediumAddendSorting(query, input, out var hasSorting);
        if (hasSorting)
        {
            return sorted;
        }

        return base.ApplySorting(query, input);
    }

    protected override async Task<TEntity> GetEntityByIdAsync(Guid id)
    {
        return (await Repository.WithDetailsAsync()).FirstOrDefault(x => x.Id == id) ??
               throw new EntityNotFoundException(typeof(TEntity), (object)id);
    }
}