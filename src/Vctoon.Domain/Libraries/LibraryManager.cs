using Vctoon.Comics;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace Vctoon.Libraries;

public class LibraryManager(
    ILibraryRepository libraryRepository,
    IArchiveInfoRepository archiveInfoRepository,
    IComicRepository comicRepository,
    IIdentityUserRepository identityUserRepository,
    IIdentityRoleRepository identityRoleRepository,
    ILibraryPermissionRepository libraryPermissionRepository,
    ILibraryPermissionStore libraryPermissionStore) : DomainService
{
    public async Task DeleteLibraryEmptyComicAsync(Library library, bool autoSave = false)

    {
        List<Guid> deleteComicIds = await AsyncExecuter.ToListAsync(
            (await comicRepository.GetQueryableAsync())
            .Where(x => x.LibraryId == library.Id)
            .Where(x => !x.Images.Any())
            .Select(x => x.Id)
        );

        await comicRepository.DeleteManyAsync(deleteComicIds, autoSave);
    }

    public async Task<Library> CreateLibraryAsync(
        string name,
        List<string> paths
    )
    {
        var library = new Library(
            GuidGenerator.Create(),
            name
        );
        library.AddPaths(paths.Select(x => new LibraryPath(GuidGenerator.Create(), x, true, library.Id)).ToList());

        await CheckLibraryNameIsExistAsync(name);
        library = await libraryRepository.InsertAsync(library);

        return library;
    }

    public async Task SetOrUpdateLibraryPermissionAsync(
        Guid libraryId,
        Guid userId,
        bool canDownload,
        bool canComment,
        bool canStar,
        bool canView,
        bool canShare
    )
    {
        List<string>? roleNames = await identityUserRepository.GetRoleNamesAsync(userId);
        if (roleNames.Contains(VctoonSharedConsts.AdminUserRoleName))
        {
            throw new BusinessException(VctoonDomainErrorCodes.AdminCanNotChangePermission);
        }

        LibraryPermission permission = await libraryPermissionRepository.FirstOrDefaultAsync(x =>
            x.LibraryId == libraryId && x.UserId == userId) ?? new LibraryPermission(libraryId, userId, canDownload, canComment,
            canStar,
            canView, canShare);

        permission.CanDownload = canDownload;
        permission.CanComment = canComment;
        permission.CanStar = canStar;
        permission.CanView = canView;
        permission.CanShare = canShare;
        await libraryPermissionStore.SetAsync(permission);
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