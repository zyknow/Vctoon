using Vctoon.Mediums;
using Volo.Abp.Domain.Services;

namespace Vctoon.Libraries;

public class LibraryStore(
    ILibraryRepository libraryRepository,
    IComicRepository comicRepository) : DomainService
{
    public async Task DeleteLibraryEmptyComicAsync(Library library, bool autoSave = false)
    {
        var deleteComicIds = await AsyncExecuter.ToListAsync(
            (await comicRepository.GetQueryableAsync())
            .Where(x => x.LibraryId == library.Id)
            .Where(x => !x.ComicImages.Any())
            .Select(x => x.Id)
        );

        await comicRepository.DeleteManyAsync(deleteComicIds, autoSave);
    }

    public async Task<Library> CreateLibraryAsync(
        string name,
        MediumType mediumType,
        List<string> paths
    )
    {
        var library = new Library(
            GuidGenerator.Create(),
            name,
            mediumType
        );
        library.AddPaths(paths.Select(x => new LibraryPath(GuidGenerator.Create(), x, true, library.Id)).ToList());

        await CheckLibraryNameIsExistAsync(name);
        library = await libraryRepository.InsertAsync(library);

        return library;
    }

    public async Task UpdateLibraryAsync(
        Library library
    )
    {
        await CheckLibraryNameIsExistAsync(library.Name, library.Id);
    }

    public async Task UpdateLibraryPathsAsync(
        Library library,
        List<string> paths
    )
    {
        await libraryRepository.EnsureCollectionLoadedAsync(library, x => x.Paths);

        var libraryPaths = library.Paths.Where(x => x.IsRoot).Select(x => x.Path).ToList();

        var removePaths = libraryPaths.Where(x => !paths.Contains(x)).ToList();

        library.RemovePaths(removePaths);

        var addPaths = paths
            .Where(x =>
                !libraryPaths
                    .Select(x => x)
                    .Contains(x))
            .Select(x =>
                new LibraryPath(GuidGenerator.Create(),
                    x,
                    true,
                    library.Id))
            .ToList();

        library.AddPaths(addPaths);
    }

    private async Task CheckLibraryNameIsExistAsync(
        string name,
        Guid? excludeId = null
    )
    {
        var query = await libraryRepository.GetQueryableAsync();
        if (await AsyncExecuter.AnyAsync(
                query
                    .WhereIf(excludeId.HasValue, x => x.Id != excludeId)
                    .Where(x => x.Name == name)
            ))
        {
            throw new BusinessException(VctoonDomainErrorCodes.NameIsAlreadyExists).WithData("Name", name);
        }
    }
}