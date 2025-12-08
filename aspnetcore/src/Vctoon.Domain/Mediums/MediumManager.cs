using Volo.Abp.Domain.Services;
using Volo.Abp.Linq;

namespace Vctoon.Mediums;

public class MediumManager(
    IMediumRepository mediumRepository,
    IAsyncQueryableExecuter asyncQueryableExecuter) : DomainService
{
    public async Task DeleteMediumByLibraryIdAsync(Guid libraryId, bool autoSave = false)
    {
        var mediumIds = await asyncQueryableExecuter.ToListAsync((await mediumRepository.GetQueryableAsync())
            .Where(x => x.LibraryId == libraryId)
            .Select(x => x.Id));

        if (!mediumIds.IsNullOrEmpty())
            await mediumRepository.DeleteManyAsync(mediumIds, autoSave);
    }

    public async Task DeleteMediumByLibraryPathIdsAsync(IEnumerable<Guid> libraryPathIds, bool autoSave = false)
    {
        var mediumIds = await asyncQueryableExecuter.ToListAsync((await mediumRepository.GetQueryableAsync())
            .Where(x => x.LibraryPathId.HasValue && libraryPathIds.Contains(x.LibraryPathId.Value))
            .Select(x => x.Id));

        if (!mediumIds.IsNullOrEmpty())
            await mediumRepository.DeleteManyAsync(mediumIds, autoSave);
    }
}