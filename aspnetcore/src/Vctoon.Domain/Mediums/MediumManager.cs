using Volo.Abp.Domain.Services;
using Volo.Abp.Linq;

namespace Vctoon.Mediums;

public class MediumManager(
    IComicRepository comicRepository,
    IVideoRepository videoRepository,
    IAsyncQueryableExecuter asyncQueryableExecuter) : DomainService
{
    public async Task DeleteMediumByLibraryIdAsync(Guid libraryId, bool autoSave = false)
    {
        var comicIds = await asyncQueryableExecuter.ToListAsync((await comicRepository.GetQueryableAsync())
            .Where(x => x.LibraryId == libraryId)
            .Select(x => x.Id));

        var videoIds = await asyncQueryableExecuter.ToListAsync((await videoRepository.GetQueryableAsync())
            .Where(x => x.LibraryId == libraryId)
            .Select(x => x.Id));

        if (!comicIds.IsNullOrEmpty())
            await comicRepository.DeleteManyAsync(comicIds, autoSave);
        if (!videoIds.IsNullOrEmpty())
            await videoRepository.DeleteManyAsync(videoIds, autoSave);
    }

    public async Task DeleteMediumByLibraryPathIdsAsync(IEnumerable<Guid> libraryPathIds, bool autoSave = false)
    {
        var comicIds = await asyncQueryableExecuter.ToListAsync((await comicRepository.GetQueryableAsync())
            .Where(x => libraryPathIds.Contains(x.LibraryPathId))
            .Select(x => x.Id));

        var videoIds = await asyncQueryableExecuter.ToListAsync((await videoRepository.GetQueryableAsync())
            .Where(x => libraryPathIds.Contains(x.LibraryPathId))
            .Select(x => x.Id));

        if (!comicIds.IsNullOrEmpty())
            await comicRepository.DeleteManyAsync(comicIds, autoSave);
        if (!videoIds.IsNullOrEmpty())
            await videoRepository.DeleteManyAsync(videoIds, autoSave);
    }
}