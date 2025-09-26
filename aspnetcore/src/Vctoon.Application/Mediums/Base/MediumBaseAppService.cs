using System.Linq.Expressions;
using Vctoon.Mediums.Dtos.Base;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Vctoon.Extensions;
using Volo.Abp.Content; // 新增 RemoteStreamContent
using Volo.Abp; // UserFriendlyException
using Vctoon.Services; // CoverSaver
using Vctoon.Libraries; // Tag / Artist
using Vctoon.Mediums.Base;

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
    // 由派生类提供用于标识“当前媒资类型绑定键”的属性选择器，例如 p => p.ComicId 或 p => p.VideoId
    protected abstract LambdaExpression ProcessKeySelector { get; }

    // 可重写：默认用属性选择器做 NotNull 判断；派生类如需 IsNull/Equal/组合条件，可覆盖此方法。
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
            if (input.ReadingProgressType != null)
            {
                var isNotNullPredicate = ProcessKeySelector.NotNull();

                query = input.ReadingProgressType switch
                {
                    ReadingProgressType.NotStarted => query.Where(x =>
                        !x.Processes.AsQueryable()
                            .Where(isNotNullPredicate)
                            .Any(p => p.UserId == userId && p.Progress > 0)
                    ),
                    ReadingProgressType.InProgress => query.Where(x => x.Processes.AsQueryable()
                        .Where(isNotNullPredicate)
                        .Any(p => p.UserId == userId && p.Progress > 0 && p.Progress < 1)),
                    ReadingProgressType.Completed => query.Where(x => x.Processes.AsQueryable()
                        .Where(isNotNullPredicate)
                        .Any(p => p.UserId == userId && p.Progress >= 1)),
                    _ => query
                };
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
    
    // 延迟解析依赖（不破坏派生类构造函数）
    protected CoverSaver CoverSaver => LazyServiceProvider.LazyGetRequiredService<CoverSaver>();
    protected ITagRepository TagRepository => LazyServiceProvider.LazyGetRequiredService<ITagRepository>();
    protected IArtistRepository ArtistRepository => LazyServiceProvider.LazyGetRequiredService<IArtistRepository>();

    public virtual async Task<TGetOutputDto> UpdateCoverAsync(Guid id, RemoteStreamContent cover)
    {
        await CheckUpdatePolicyAsync();

        if (cover == null)
        {
            throw new UserFriendlyException("封面流不能为空");
        }

        var entity = await GetEntityByIdAsync(id);

        // 保存新封面
        await using var stream = cover.GetStream();
        if (stream == null || stream.Length == 0)
        {
            throw new UserFriendlyException("封面流为空");
        }

        var newCover = await CoverSaver.SaveAsync(stream);
        entity.Cover = newCover;

        await Repository.UpdateAsync(entity, true);
        return await MapToGetOutputDtoAsync(entity);
    }
    
    public virtual async Task<TGetOutputDto> UpdateArtistsAsync(Guid id, List<Guid> artistIds)
    {
        await CheckUpdatePolicyAsync();

        artistIds ??= new List<Guid>();
        artistIds = artistIds.Distinct().Where(g => g != Guid.Empty).ToList();

        var entity = await GetEntityByIdAsync(id);

        // 查询需要的作者
        var artistQuery = await ArtistRepository.GetQueryableAsync();
        var artists = await AsyncExecuter.ToListAsync(artistQuery.Where(a => artistIds.Contains(a.Id)));

        // 替换集合（移除不存在的，添加新的）
        entity.Artists.RemoveAll(a => !artistIds.Contains(a.Id));
        foreach (var artist in artists)
        {
            if (entity.Artists.All(a => a.Id != artist.Id))
            {
                entity.Artists.Add(artist);
            }
        }

        await Repository.UpdateAsync(entity, true);
        return await MapToGetOutputDtoAsync(entity);
    }
    
    public virtual async Task<TGetOutputDto> UpdateTagsAsync(Guid id, List<Guid> tagIds)
    {
        await CheckUpdatePolicyAsync();

        tagIds ??= new List<Guid>();
        tagIds = tagIds.Distinct().Where(g => g != Guid.Empty).ToList();

        var entity = await GetEntityByIdAsync(id);

        var tagQuery = await TagRepository.GetQueryableAsync();
        var tags = await AsyncExecuter.ToListAsync(tagQuery.Where(t => tagIds.Contains(t.Id)));

        // 移除未包含的标签
        entity.Tags.RemoveAll(t => !tagIds.Contains(t.Id));
        foreach (var tag in tags)
        {
            if (entity.Tags.All(t => t.Id != tag.Id))
            {
                entity.Tags.Add(tag);
            }
        }

        await Repository.UpdateAsync(entity, true);
        return await MapToGetOutputDtoAsync(entity);
    }
}