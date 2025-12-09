using Vctoon.BlobContainers;
using Vctoon.Identities;
using Vctoon.ImageProviders;
using Vctoon.Mediums.Dtos;
using Volo.Abp;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Vctoon.Mediums;

public partial class MediumAppService(
    IArchiveInfoRepository archiveInfoRepository,
    IImageProvider imageProvider,
    IMediumRepository repository,
    IRepository<ComicImage, Guid> comicImageRepository,
    IBlobContainer<CoverContainer> coverContainer,
    IIdentityUserReadingProcessRepository readingProcessRepository,
    IMediumRepository mediumRepository,
    IRepository<Artist, Guid> artistRepository,
    ITagRepository tagRepository) : VctoonCrudAppService<
    Medium,
    MediumDto,
    Guid,
    MediumGetListInput,
    CreateUpdateMediumDto, CreateUpdateMediumDto>(repository), IMediumAppService
{
    protected CoverSaver CoverSaver => LazyServiceProvider.LazyGetRequiredService<CoverSaver>();
    protected ITagRepository TagRepository => LazyServiceProvider.LazyGetRequiredService<ITagRepository>();
    protected IArtistRepository ArtistRepository => LazyServiceProvider.LazyGetRequiredService<IArtistRepository>();


    [RemoteService(false)]
    public override Task<MediumDto> CreateAsync(CreateUpdateMediumDto input)
    {
        throw new NotImplementedException();
    }


    protected override async Task<IQueryable<Medium>> CreateFilteredQueryAsync(MediumGetListInput input)
    {
        var query = await repository.WithUserPageDetailsAsync(CurrentUser.Id);

        // TODO: AbpHelper generated
        query = query
                .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Title.Contains(input.Filter!))
                .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Description.Contains(input.Filter!))
                .WhereIf(input.LibraryId != null, x => x.LibraryId == input.LibraryId)
                .WhereIf(!input.Artists.IsNullOrEmpty(), x => x.Artists.Any(a => input.Artists!.Contains(a.Id)))
                .WhereIf(!input.Tags.IsNullOrEmpty(), x => x.Tags.Any(t => input.Tags!.Contains(t.Id)))
            ;

        if (CurrentUser.Id != null)
        {
            var userId = CurrentUser.Id.Value;
            if (input.ReadingProgressType != null)
            {
                query = input.ReadingProgressType switch
                {
                    ReadingProgressType.NotStarted => query.Where(x =>
                        !x.Processes.AsQueryable()
                            .Any(p => p.UserId == userId && p.Progress > 0)
                    ),
                    ReadingProgressType.InProgress => query.Where(x => x.Processes.AsQueryable()
                        .Any(p => p.UserId == userId && p.Progress > 0 && p.Progress < 1)),
                    ReadingProgressType.Completed => query.Where(x => x.Processes.AsQueryable()
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
        else if (field.Equals(nameof(MediumGetListOutputDto.ReadingProgress), StringComparison.OrdinalIgnoreCase))
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
        return (await repository.WithUserDetailsAsync(CurrentUser.Id))
               .FirstOrDefault(x => x.Id == id) ??
               throw new EntityNotFoundException(typeof(Medium), id);
    }


    public virtual async Task<MediumDto> UpdateCoverAsync(Guid id, RemoteStreamContent cover)
    {
        await CheckUpdatePolicyAsync();

        if (cover == null)
        {
            throw new UserFriendlyException("cover is null");
        }

        var entity = await GetEntityByIdAsync(id);

        await using var stream = cover.GetStream();
        if (stream == null || stream.Length == 0)
        {
            throw new UserFriendlyException("cover stream is empty");
        }

        var newCover = await CoverSaver.SaveAsync(stream);
        entity.Cover = newCover;

        await Repository.UpdateAsync(entity, true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public virtual async Task<MediumDto> UpdateArtistsAsync(Guid id, List<Guid> artistIds)
    {
        await CheckUpdatePolicyAsync();

        artistIds ??= new List<Guid>();
        artistIds = artistIds.Distinct().Where(g => g != Guid.Empty).ToList();

        var entity = await GetEntityByIdAsync(id);

        var artistQuery = await ArtistRepository.GetQueryableAsync();
        var artists = await AsyncExecuter.ToListAsync(artistQuery.Where(a => artistIds.Contains(a.Id)));

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

    public virtual async Task<MediumDto> UpdateTagsAsync(Guid id, List<Guid> tagIds)
    {
        await CheckUpdatePolicyAsync();

        tagIds ??= new List<Guid>();
        tagIds = tagIds.Distinct().Where(g => g != Guid.Empty).ToList();

        var entity = await GetEntityByIdAsync(id);

        var tagQuery = await TagRepository.GetQueryableAsync();
        var tags = await AsyncExecuter.ToListAsync(tagQuery.Where(t => tagIds.Contains(t.Id)));

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