using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Uow;

namespace Vctoon.Libraries;

public class LibraryPermissionStore : ISingletonDependency
{
    private readonly ILibraryPermissionRepository _libraryPermissionRepository;

    public List<LibraryPermission> Permissions { get;  }

    public LibraryPermissionStore(
        ILibraryPermissionRepository libraryPermissionRepository)
    {
        _libraryPermissionRepository = libraryPermissionRepository;
        Permissions = libraryPermissionRepository.GetListAsync().Result;
    }

    public async Task<LibraryPermission?> GetOrNullAsync(Guid userId, Guid libraryId)
    {
        return Permissions.FirstOrDefault(x => x.UserId == userId && x.LibraryId == libraryId);
    }

     
    
    public async Task RemoveUserPermissionAsync(Guid userId, Guid libraryId)
    {
        await _libraryPermissionRepository.DeleteDirectAsync(x => x.UserId == userId);
        Permissions.RemoveAll(x => x.UserId == userId);
    }

    [UnitOfWork]
    public async Task SetAsync(LibraryPermission permission)
    {
        var existingPermission =
            await _libraryPermissionRepository.FirstOrDefaultAsync(x =>
                x.UserId == permission.UserId && x.LibraryId == permission.LibraryId);

        if (existingPermission == null)
        {
            await _libraryPermissionRepository.InsertAsync(permission);
        }
        else
        {
            existingPermission.CanDownload = permission.CanDownload;
            existingPermission.CanComment = permission.CanComment;
            existingPermission.CanStar = permission.CanStar;
            existingPermission.CanView = permission.CanView;
            existingPermission.CanShare = permission.CanShare;

            await _libraryPermissionRepository.UpdateAsync(existingPermission);
        }

        Permissions.RemoveAll(x => x.UserId == permission.UserId && x.LibraryId == permission.LibraryId);
        Permissions.Add(permission);
    }
}