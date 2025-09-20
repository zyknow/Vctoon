using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Uow;

namespace Vctoon.Libraries;

public class LibraryPermissionStore(
    IDistributedCache<LibraryPermission> cache,
    ILibraryPermissionRepository libraryPermissionRepository,
    ILibraryRepository libraryRepository,
    IIdentityUserRepository identityUserRepository)
    : ITransientDependency
{
    private static string CalculateCacheKey(Guid userId, Guid libraryId)
    {
        return "li:" + libraryId + ",ui:" + userId;
    }

    public async Task<LibraryPermission?> GetOrNullAsync(Guid userId, Guid libraryId)
    {
        var key = CalculateCacheKey(userId, libraryId);

        return await cache.GetOrAddAsync(key, async () =>
        {
            var permission = await libraryPermissionRepository.FirstOrDefaultAsync(x =>
                x.UserId == userId && x.LibraryId == libraryId);
            return permission;
        });
    }

    public async Task RemoveUserPermissionAsync(Guid userId, Guid libraryId)
    {
        var key = CalculateCacheKey(userId, libraryId);
        await libraryPermissionRepository.DeleteDirectAsync(x => x.UserId == userId);
        await cache.RemoveAsync(key);
    }

    [UnitOfWork]
    public async Task SetAsync(LibraryPermission permission)
    {
        var existingPermission =
            await libraryPermissionRepository.FirstOrDefaultAsync(x =>
                x.UserId == permission.UserId && x.LibraryId == permission.LibraryId);

        if (existingPermission == null)
        {
            await libraryPermissionRepository.InsertAsync(permission);
        }
        else
        {
            existingPermission.CanDownload = permission.CanDownload;
            existingPermission.CanComment = permission.CanComment;
            existingPermission.CanStar = permission.CanStar;
            existingPermission.CanView = permission.CanView;
            existingPermission.CanShare = permission.CanShare;

            await libraryPermissionRepository.UpdateAsync(existingPermission);
        }

        await cache.SetAsync(CalculateCacheKey(permission.UserId, permission.LibraryId),
            permission);
    }
}